using FileSorterByDate;

namespace FileSorterByDate.Tests;

public class DestinationPathBuilderTests
{
	[Fact]
	public void Build_UsesYearAndUnpaddedMonth()
	{
		var root = Path.Combine(Path.GetTempPath(), "Rullino");
		var path = DestinationPathBuilder.Build(root, new DateTime(2024, 3, 15, 14, 30, 22));

		Assert.Equal(Path.Combine(root, "2024", "3"), path);
	}
}
