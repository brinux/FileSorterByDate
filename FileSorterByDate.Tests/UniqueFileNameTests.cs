using FileSorterByDate;

namespace FileSorterByDate.Tests;

public class UniqueFileNameTests
{
	[Fact]
	public void GetAvailablePath_WhenFileDoesNotExist_ReturnsOriginalPath()
	{
		var directory = Directory.CreateTempSubdirectory().FullName;

		try
		{
			var path = UniqueFileName.GetAvailablePath(directory, "photo.jpg");

			Assert.Equal(Path.Combine(directory, "photo.jpg"), path);
		}
		finally
		{
			Directory.Delete(directory, true);
		}
	}

	[Fact]
	public void GetAvailablePath_WhenFileExists_AppendsNumericSuffix()
	{
		var directory = Directory.CreateTempSubdirectory().FullName;

		try
		{
			File.WriteAllText(Path.Combine(directory, "photo.jpg"), "a");
			File.WriteAllText(Path.Combine(directory, "photo_1.jpg"), "b");

			var path = UniqueFileName.GetAvailablePath(directory, "photo.jpg");

			Assert.Equal(Path.Combine(directory, "photo_2.jpg"), path);
		}
		finally
		{
			Directory.Delete(directory, true);
		}
	}
}
