using Vixen.Extensions;
using Vixen.Sys.Props.Model;

namespace VixenModules.App.Props.Models.Arch
{
	public class ArchModel: BaseLightModel
	{
		private Arch _arch;

		public ArchModel(Arch arch)
        {
			_arch = arch;
            Nodes.AddRange<NodePoint>(GetArchPoints(_arch.NodeCount, _arch.LightSize, _arch.Rotation));
        }

		public override void DrawModel()
		{
			Nodes.Clear();
			Nodes.AddRange<NodePoint>(GetArchPoints(_arch.NodeCount, _arch.LightSize, _arch.Rotation));
		}

		private static List<NodePoint> GetArchPoints(double numPoints, int size, int rotationAngle)
        {
            List<NodePoint> vertices = new List<NodePoint>();
            double xScale = .5f;
            double yScale = 1;
            double radianIncrement = Math.PI / (numPoints - 1);

			double t = Math.PI;
			while (vertices.Count < numPoints)
			{
				double x = (xScale + xScale * Math.Cos(t));
				double y = (yScale + yScale * Math.Sin(t));
				vertices.Add(new NodePoint(x, y) { Size = size });
				t += radianIncrement;
			}

			if (rotationAngle != 0)
            {
                RotateNodePoints(vertices, rotationAngle);
            }

			// We need to adjust the position of the final points to allow sufficient area to show the full light size
			double reduction = 1 / Math.Pow(size,0.03);
			foreach (var node in vertices)
			{
				node.X *= reduction;
				node.Y *= reduction;
			}

            return vertices;
        }
	}
}
