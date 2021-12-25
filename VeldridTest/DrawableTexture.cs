using System.Numerics;

namespace VeldridTest {
	public class DrawableTexture : Drawable {
		public Texture2D Texture { get; protected set; }

		public Vector2 Scale = new(1f);
		
		public DrawableTexture(Vector2 position, Texture2D texture) {
			this.Position = position;
			this.Texture  = texture;
		}
		
		public override void Draw(RenderState renderState) {
			Renderer.DrawTexture(this.Texture, this.Position, this.Color, new Vector2(this.Texture.Size.X, this.Texture.Size.Y) * this.Scale);
		}
		
		public override void Dispose() {
			
		}
	}
}
