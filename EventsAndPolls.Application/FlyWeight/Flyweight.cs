namespace EventsAndPolls.Application.Flyweight;

public class TreeType
{
     // Intrinsic state — identical for all trees of this species
     public string Species { get; }
     public string Color { get; }
     public string Texture { get; }

     public TreeType(string species, string color, string texture)
     {
          Species = species;
          Color = color;
          Texture = texture;
          Console.WriteLine($"[Flyweight] TreeType created for species: {species} (stored once in memory)");
     }

     // Render uses extrinsic state (position, size) passed in by the caller
     public void Render(int x, int y, int height)
     {
          Console.WriteLine($"[Flyweight] Rendering {Species} tree at ({x},{y}) " +
                            $"height={height} color={Color} texture={Texture}");
     }
}

public class TreeTypeFactory
{
     private readonly Dictionary<string, TreeType> _treeTypes = new();

     public TreeType GetTreeType(string species, string color, string texture)
     {
          if (!_treeTypes.ContainsKey(species))
          {
               _treeTypes[species] = new TreeType(species, color, texture);
          }
          else
          {
               Console.WriteLine($"[Flyweight] Reusing existing TreeType for: {species}");
          }

          return _treeTypes[species];
     }

     public int UniqueTypesCount => _treeTypes.Count;
}

public class Tree
{
     // Extrinsic state — unique per tree
     public int X { get; }
     public int Y { get; }
     public int Height { get; }

     // Reference to shared flyweight — NOT a copy
     private readonly TreeType _type;

     public Tree(int x, int y, int height, TreeType type)
     {
          X = x;
          Y = y;
          Height = height;
          _type = type;
     }

     public void Render()
     {
          // Passes its own extrinsic state to the shared flyweight
          _type.Render(X, Y, Height);
     }
}

// Forest — client that creates thousands of trees efficiently
public class Forest
{
     private readonly List<Tree> _trees = new();
     private readonly TreeTypeFactory _factory = new();

     public void PlantTree(int x, int y, int height, string species, string color, string texture)
     {
          // Get or reuse the shared flyweight for this species
          var type = _factory.GetTreeType(species, color, texture);
          _trees.Add(new Tree(x, y, height, type));
     }

     public void RenderAll()
     {
          Console.WriteLine($"[Forest] Rendering {_trees.Count} trees " +
                            $"using only {_factory.UniqueTypesCount} unique TreeType objects in memory");
          foreach (var tree in _trees)
               tree.Render();
     }
}

//   Exemplu utilizare
//   var forest = new Forest();
//   // Plant 100,000 trees — only 3 TreeType objects created in memory
//   for (int i = 0; i < 50000; i++) forest.PlantTree(i, i*2, 10, "Oak",   "#5D4037", "oak_texture.png");
//   for (int i = 0; i < 30000; i++) forest.PlantTree(i, i*3, 8,  "Pine",  "#2E7D32", "pine_texture.png");
//   for (int i = 0; i < 20000; i++) forest.PlantTree(i, i*4, 6,  "Birch", "#FAFAFA", "birch_texture.png");
//   forest.RenderAll();
//   // Output: Rendering 100,000 trees using only 3 unique TreeType objects in memory