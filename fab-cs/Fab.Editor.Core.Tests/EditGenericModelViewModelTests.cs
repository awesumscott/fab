using Fab.Data;
using Fab.Editor.Core.Services;
using Fab.Editor.Core.ViewModels;
using Shouldly;

namespace Fab.Editor.Core.Tests;

public class EditGenericModelViewModelTests {
	[Fact]
	public void BuildFields_Article_ProducesTextFieldForTitleAndListForEntries() {
		var article = new Article { Title = "Hello" };

		var vm = new EditGenericModelViewModel(article);

		vm.Fields.OfType<EditTextFieldViewModel>().ShouldContain(f => f.Name == nameof(Article.Title));
		vm.Fields.OfType<EditListViewModel>().ShouldContain(f => f.Name == nameof(Article.Entries));
	}

	[Fact]
	public void BuildFields_SkipsIntProperties() {
		var article = new Article { Title = "X" };

		var vm = new EditGenericModelViewModel(article);

		vm.Fields.OfType<EditTextFieldViewModel>()
			.Select(f => f.Name)
			.ShouldNotContain(nameof(Article.Id));
	}

	[Fact]
	public void TextField_SetText_WritesBackToModel() {
		var paragraph = new Paragraph { Text = "original" };
		var vm = new EditGenericModelViewModel(paragraph);
		var textField = vm.Fields.OfType<EditTextFieldViewModel>().Single(f => f.Name == nameof(Paragraph.Text));

		textField.Text = "updated";

		paragraph.Text.ShouldBe("updated");
	}

	[Fact]
	public void List_ExposesChildEditorsForEachItem() {
		var article = new Article {
			Title = "T",
			Entries = {
				new OrderedContentEntry(0, new Paragraph { Text = "A" }),
				new OrderedContentEntry(1, new Paragraph { Text = "B" }),
			}
		};
		var vm = new EditGenericModelViewModel(article);

		var list = vm.Fields.OfType<EditListViewModel>().Single();
		list.Items.Count.ShouldBe(2);
		list.Items.ShouldAllBe(item => item is EditGenericModelViewModel);
	}

	[Fact]
	public void ListAddOptions_ForEntries_IncludesEachContentSubclass() {
		var article = new Article { Title = "T" };
		var vm = new EditGenericModelViewModel(article);
		var list = vm.Fields.OfType<EditListViewModel>().Single(l => l.Name == nameof(Article.Entries));

		var labels = list.AddOptions.Select(o => o.Label).ToList();
		labels.ShouldContain(nameof(Paragraph));
		labels.ShouldContain(nameof(Image));
	}

	[Fact]
	public void ListAdd_AppendsEntryWithCorrectContentType() {
		var article = new Article { Title = "T" };
		var vm = new EditGenericModelViewModel(article);
		var list = vm.Fields.OfType<EditListViewModel>().Single(l => l.Name == nameof(Article.Entries));
		var paragraphOption = list.AddOptions.Single(o => o.Label == nameof(Paragraph));

		list.AddCommand.Execute(paragraphOption);

		article.Entries.Count.ShouldBe(1);
		article.Entries[0].Content.ShouldBeOfType<Paragraph>();
		article.Entries[0].Order.ShouldBe(0);
		list.Items.Count.ShouldBe(1);
	}

	[Fact]
	public void ListDelete_RemovesEntryAndRenumbers() {
		var article = new Article {
			Entries = {
				new(0, new Paragraph { Text = "A" }),
				new(1, new Paragraph { Text = "B" }),
				new(2, new Paragraph { Text = "C" }),
			}
		};
		var list = ListOf(article);

		list.DeleteCommand.Execute(list.Items[1]); // remove B

		article.Entries.Count.ShouldBe(2);
		((Paragraph)article.Entries[0].Content).Text.ShouldBe("A");
		((Paragraph)article.Entries[1].Content).Text.ShouldBe("C");
		article.Entries[0].Order.ShouldBe(0);
		article.Entries[1].Order.ShouldBe(1);
	}

	[Fact]
	public void ListMoveUp_SwapsAndRenumbers() {
		var article = new Article {
			Entries = {
				new(0, new Paragraph { Text = "A" }),
				new(1, new Paragraph { Text = "B" }),
			}
		};
		var list = ListOf(article);

		list.MoveUpCommand.Execute(list.Items[1]); // move B up

		((Paragraph)article.Entries[0].Content).Text.ShouldBe("B");
		((Paragraph)article.Entries[1].Content).Text.ShouldBe("A");
		article.Entries[0].Order.ShouldBe(0);
		article.Entries[1].Order.ShouldBe(1);
	}

	[Fact]
	public void ListMoveDown_SwapsAndRenumbers() {
		var article = new Article {
			Entries = {
				new(0, new Paragraph { Text = "A" }),
				new(1, new Paragraph { Text = "B" }),
			}
		};
		var list = ListOf(article);

		list.MoveDownCommand.Execute(list.Items[0]); // move A down

		((Paragraph)article.Entries[0].Content).Text.ShouldBe("B");
		((Paragraph)article.Entries[1].Content).Text.ShouldBe("A");
	}

	[Fact]
	public void ListMoveUp_OnFirstItem_IsNoOp() {
		var article = new Article {
			Entries = {
				new(0, new Paragraph { Text = "A" }),
				new(1, new Paragraph { Text = "B" }),
			}
		};
		var list = ListOf(article);

		list.MoveUpCommand.Execute(list.Items[0]);

		((Paragraph)article.Entries[0].Content).Text.ShouldBe("A");
		((Paragraph)article.Entries[1].Content).Text.ShouldBe("B");
	}

	[Fact]
	public void ListMoveDown_OnLastItem_IsNoOp() {
		var article = new Article {
			Entries = {
				new(0, new Paragraph { Text = "A" }),
				new(1, new Paragraph { Text = "B" }),
			}
		};
		var list = ListOf(article);

		list.MoveDownCommand.Execute(list.Items[1]);

		((Paragraph)article.Entries[0].Content).Text.ShouldBe("A");
		((Paragraph)article.Entries[1].Content).Text.ShouldBe("B");
	}

	private static EditListViewModel ListOf(Article article, IConfirmationService? confirmation = null) =>
		new EditGenericModelViewModel(article, confirmation).Fields.OfType<EditListViewModel>().Single(l => l.Name == nameof(Article.Entries));

	[Fact]
	public async Task Delete_PromptsConfirmation_AndDeclineKeepsItem() {
		var article = new Article {
			Entries = {
				new(0, new Paragraph { Text = "A" }),
				new(1, new Paragraph { Text = "B" }),
			}
		};
		var confirmer = new StubConfirmationService(result: false);
		var list = ListOf(article, confirmer);

		await list.DeleteCommand.ExecuteAsync(list.Items[0]);

		confirmer.CallCount.ShouldBe(1);
		article.Entries.Count.ShouldBe(2);
	}

	[Fact]
	public async Task Delete_Confirmed_RemovesItem() {
		var article = new Article {
			Entries = {
				new(0, new Paragraph { Text = "A" }),
				new(1, new Paragraph { Text = "B" }),
			}
		};
		var confirmer = new StubConfirmationService(result: true);
		var list = ListOf(article, confirmer);

		await list.DeleteCommand.ExecuteAsync(list.Items[0]);

		confirmer.CallCount.ShouldBe(1);
		article.Entries.Count.ShouldBe(1);
		((Paragraph)article.Entries[0].Content).Text.ShouldBe("B");
	}

	[Fact]
	public void MoveBefore_ReordersAndRenumbers() {
		var article = new Article {
			Entries = {
				new(0, new Paragraph { Text = "A" }),
				new(1, new Paragraph { Text = "B" }),
				new(2, new Paragraph { Text = "C" }),
			}
		};
		var list = ListOf(article);
		var source = (EditGenericModelViewModel)list.Items[2];
		var target = (EditGenericModelViewModel)list.Items[0];

		list.MoveBefore(source, target);

		((Paragraph)article.Entries[0].Content).Text.ShouldBe("C");
		((Paragraph)article.Entries[1].Content).Text.ShouldBe("A");
		((Paragraph)article.Entries[2].Content).Text.ShouldBe("B");
		article.Entries[0].Order.ShouldBe(0);
		article.Entries[1].Order.ShouldBe(1);
		article.Entries[2].Order.ShouldBe(2);
	}

	private sealed class StubConfirmationService : IConfirmationService {
		private readonly bool _result;
		public int CallCount { get; private set; }
		public StubConfirmationService(bool result) => _result = result;
		public Task<bool> ConfirmAsync(string message, string title = "Confirm") {
			CallCount++;
			return Task.FromResult(_result);
		}
		public Task<UnsavedChangesResult> PromptUnsavedAsync(string message, string title = "Unsaved changes") =>
			Task.FromResult(UnsavedChangesResult.Cancel);
	}

	[Fact]
	public void OnChanged_FiresWhenTextFieldIsModified() {
		var paragraph = new Paragraph { Text = "original" };
		int count = 0;
		var vm = new EditGenericModelViewModel(paragraph, onChanged: () => count++);

		vm.Fields.OfType<EditTextFieldViewModel>().Single().Text = "new";

		count.ShouldBe(1);
	}

	[Fact]
	public void OnChanged_FiresForNestedTextEdit() {
		var article = new Article {
			Entries = { new(0, new Paragraph { Text = "A" }) }
		};
		int count = 0;
		var vm = new EditGenericModelViewModel(article, onChanged: () => count++);
		var list = vm.Fields.OfType<EditListViewModel>().Single();
		var entry = (EditGenericModelViewModel)list.Items[0];
		var contentEditor = entry.Fields.OfType<EditGenericModelViewModel>().Single();
		var text = contentEditor.Fields.OfType<EditTextFieldViewModel>().Single();

		text.Text = "updated";

		count.ShouldBe(1);
	}

	[Fact]
	public void OnChanged_FiresOnListAdd() {
		var article = new Article();
		int count = 0;
		var vm = new EditGenericModelViewModel(article, onChanged: () => count++);
		var list = vm.Fields.OfType<EditListViewModel>().Single();

		list.AddCommand.Execute(list.AddOptions.First());

		count.ShouldBe(1);
	}

	[Fact]
	public void BuildFields_RecursesIntoNestedUnique() {
		var entry = new OrderedContentEntry(0, new Paragraph { Text = "nested" });

		var vm = new EditGenericModelViewModel(entry);

		var nested = vm.Fields.OfType<EditGenericModelViewModel>().SingleOrDefault();
		nested.ShouldNotBeNull();
		nested.Model.ShouldBeOfType<Paragraph>();
		nested.Fields.OfType<EditTextFieldViewModel>()
			.ShouldContain(f => f.Name == nameof(Paragraph.Text));
	}
}
