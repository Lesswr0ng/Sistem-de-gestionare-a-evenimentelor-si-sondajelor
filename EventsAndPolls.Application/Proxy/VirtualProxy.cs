namespace EventsAndPolls.Application.Proxy;

// Proxy Pattern — Virtual Proxy (Theoretical Example)
//
// CONCEPT: Delays the creation of an expensive object until it is actually
// needed. The client interacts with the proxy as if it were the real object —
// the real object is only initialized on first use.
//
// GENERAL SCENARIO: Image loading in a document editor.
// A document may contain hundreds of high-resolution images. Loading all of
// them into memory when the document opens would be extremely slow. A
// VirtualImage proxy is created immediately for each image (cheap), but the
// actual image data is only loaded from disk when the image is first rendered.
//
// Intrinsic state created immediately: filename, dimensions
// Expensive state loaded lazily:        actual pixel data

public interface IImage
{
     void Render();
     string FileName { get; }
}

// RealImage — expensive to create, loads file from disk on construction
public class RealImage : IImage
{
     public string FileName { get; }
     private byte[] _pixelData;

     public RealImage(string fileName)
     {
          FileName = fileName;
          _pixelData = LoadFromDisk(fileName);
     }

     private byte[] LoadFromDisk(string fileName)
     {
          Console.WriteLine($"[VirtualProxy] Loading image from disk: {fileName}");
          // Simulates expensive disk I/O
          Thread.Sleep(100);
          return new byte[1024];
     }

     public void Render()
     {
          Console.WriteLine($"[VirtualProxy] Rendering image: {FileName} ({_pixelData.Length} bytes)");
     }
}

// VirtualImage — proxy that delays loading until Render() is called
public class VirtualImage : IImage
{
     public string FileName { get; }
     private RealImage? _realImage;

     public VirtualImage(string fileName)
     {
          // Cheap — no disk I/O here
          FileName = fileName;
          Console.WriteLine($"[VirtualProxy] Proxy created for: {fileName} (not loaded yet)");
     }

     public void Render()
     {
          // Lazy initialization — load only on first render
          if (_realImage == null)
          {
               Console.WriteLine($"[VirtualProxy] First render — initializing real image now");
               _realImage = new RealImage(FileName);
          }

          _realImage.Render();
     }
}

// Client — works with IImage, never knows if it's real or proxy
public class DocumentEditor
{
     private readonly List<IImage> _images = new();

     public void AddImage(string fileName)
     {
          // Add proxy — no disk I/O yet even if 100 images are added
          _images.Add(new VirtualImage(fileName));
          Console.WriteLine($"[DocumentEditor] Image added to document: {fileName}");
     }

     public void RenderPage(int imageIndex)
     {
          // Only THIS image gets loaded — not all 100
          if (imageIndex < _images.Count)
               _images[imageIndex].Render();
     }
}

// Example usage:
//   var editor = new DocumentEditor();
//   editor.AddImage("photo1.jpg");   // no disk I/O
//   editor.AddImage("photo2.jpg");   // no disk I/O
//   editor.AddImage("photo3.jpg");   // no disk I/O
//   editor.RenderPage(0);            // ONLY photo1.jpg loaded from disk here