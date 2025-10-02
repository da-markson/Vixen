#nullable enable
using System.Collections.ObjectModel;
using Vixen.Model;

namespace Vixen.Sys.Props.Model
{
	public abstract class BaseLightModel : BasePropModel, ILightPropModel
	{
		private ObservableCollection<NodePoint> _nodes = new();

		public abstract void DrawModel();

		public ObservableCollection<NodePoint> Nodes
		{
			get => _nodes;
			set => SetProperty(ref _nodes, value);
		}

		/// <summary>
		/// Rotates the NodePoints around the center of a 0,1 matrix.
		/// </summary>
		/// <param name="nodePoints"></param>
		/// <param name="angleInDegrees"></param>
		protected static void RotateNodePoints(List<NodePoint> nodePoints, int angleInDegrees)
		{
			double centerX = .5;
			double centerY = .5;
			double angleInRadians = angleInDegrees * (Math.PI / 180);
			double cosTheta = Math.Cos(angleInRadians);
			double sinTheta = Math.Sin(angleInRadians);
			foreach (var nodePoint in nodePoints)
			{
				double x =
					cosTheta * (nodePoint.X - centerX) -
						sinTheta * (nodePoint.Y - centerY);
				 double y =
					sinTheta * (nodePoint.X - centerX) +
					 cosTheta * (nodePoint.Y - centerY);

				nodePoint.X = x + centerX;
				nodePoint.Y = y + centerY;
			}
		}


	}
}
