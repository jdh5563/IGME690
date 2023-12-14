/// <summary>
/// Represents one cell in the level's grid
/// </summary>
public class Cell
{
	public bool isCollapsed = false;
	public int[] options;
	public int x;
	public int y;
	public bool isRoom = false;

	public Cell(int[] options, int x, int y)
	{
		this.options = options;
		this.x = x;
		this.y = y;
	}
}
