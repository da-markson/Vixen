#nullable enable
using System.Collections.ObjectModel;
using Vixen.Extensions;

namespace Vixen.Sys.Props.Model
{
	/// <summary>
	/// Maintains a base light model.
	/// </summary>
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

		#endregion

		#region Protected Abstract Methods

		/// <summary>
		/// Retrieves the 3-D node points that make up the prop.
		/// </summary>
		/// <returns>3-D note points that make up the prop</returns>
		protected abstract IEnumerable<NodePoint> Get3DNodePoints();

		/// <summary>
		/// Retrieves the 2-D node points that make up the prop.
		/// </summary>
		/// <returns>2-D note points that make up the prop</returns>
		protected abstract IEnumerable<NodePoint> Get2DNodePoints();

		#endregion

		#region Public Properties

		private ObservableCollection<NodePoint> _nodes = new();

		public ObservableCollection<NodePoint> Nodes
		{
			get => _nodes;
			set => SetProperty(ref _nodes, value);
		}
		
		private ObservableCollection<NodePoint> _threeDNodes = new();

				nodePoint.X = x + centerX;
				nodePoint.Y = y + centerY;
			}
		}

		#endregion

		#region Public Methods

		public void UpdatePropNodes()
		{
			ThreeDNodes.Clear();
			ThreeDNodes.AddRange(Get3DNodePoints());
		}

		#endregion
	}
}
