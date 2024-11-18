public static class GlobalMaterialManager
{
    /*
    * A static class that manages the current material type for the game.
    * Used to keep track of the selected material type across different scripts.
    *
    * Author: Parker Clark
    */
    public static MaterialType CurrentMaterialType { get; set; } = MaterialType.Wood; // Default to Wood
}
