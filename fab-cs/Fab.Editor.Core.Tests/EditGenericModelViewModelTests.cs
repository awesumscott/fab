using Fab.Data;
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
