﻿using System.Runtime.Serialization;
using System.ComponentModel;
using Vixen.Sys;

namespace VixenModules.Preview.VixenPreview.Shapes
{
	[DataContract]
	public class PreviewArch : PreviewLightBaseShape
	{
		[DataMember] private PreviewPoint _topLeft;
		[DataMember] private PreviewPoint _bottomRight;

		private PreviewPoint _topRight = new PreviewPoint(10, 10);
		private PreviewPoint _bottomLeft = new PreviewPoint(10, 10);

		private PreviewPoint p1Start, p2Start;

		public override string TypeName => @"Arch";

		public PreviewArch(PreviewPoint point1, ElementNode selectedNode, double zoomLevel)
		{
			ZoomLevel = zoomLevel;
			TopLeft = PointToZoomPoint(point1).ToPoint();
			BottomRight = new PreviewPoint(_topLeft.X, _topLeft.Y).ToPoint();

			Reconfigure(selectedNode);
		}

		#region Overrides of PreviewBaseShape

		/// <inheritdoc />
		internal sealed override void Reconfigure(ElementNode node)
		{
			_pixels.Clear();
			var lightCount = 25;
			
			if (node != null)
			{
				List<ElementNode> children = PreviewTools.GetLeafNodes(node);
				// is this a single node?
				if (children.Count >= 4)
				{
					StringType = StringTypes.Pixel;
					lightCount = children.Count;
					// Just add the pixels, they will get laid out next
					foreach (ElementNode child in children)
					{
						{
							PreviewPixel pixel = AddPixel(10, 10);
							pixel.Node = child;
							pixel.PixelColor = Color.White;
						}
					}
				}
			}

			if (_pixels.Count == 0)
			{
				// Just add the pixels, they will get laid out next
				for (int lightNum = 0; lightNum < lightCount; lightNum++)
				{
					PreviewPixel pixel = AddPixel(10, 10);
					pixel.PixelColor = Color.White;
					if (node != null && node.IsLeaf)
					{
						pixel.Node = node;
					}
				}
			}

			// Lay out the pixels
			Layout();
		}

		#endregion

		[OnDeserialized]
		private new void OnDeserialized(StreamingContext context)
		{
			_topLeft.PointType = PreviewPoint.PointTypes.SizeTopLeft;
			_bottomRight.PointType = PreviewPoint.PointTypes.SizeBottomRight;
			Layout();
		}

		[Browsable(false)]
		public PreviewPoint TopRight
		{
			get
			{
                if (_topRight == null)
                    _topRight = new PreviewPoint();

				_topRight.X = _bottomRight.X;
				_topRight.Y = _topLeft.Y;
				_topRight.PointType = PreviewPoint.PointTypes.SizeTopRight;
				return _topRight;
			}
		}

		[Browsable(false)]
        public PreviewPoint BottomLeft
        {
            get
            {
                if (_bottomLeft == null)
                    _bottomLeft = new PreviewPoint();
                _bottomLeft.X = TopLeft.X;
                _bottomLeft.Y = BottomRight.Y;
				_bottomLeft.PointType = PreviewPoint.PointTypes.SizeBottomLeft;
                return _bottomLeft;
			}
		}

		[CategoryAttribute("Position"),
		 DisplayName("Top Left"),
		 DescriptionAttribute("An arch is defined by a 2 points of a rectangle. This is point 1.")]
		public Point TopLeft
		{
			get
			{
				if (_topLeft == null)
				{
					_topLeft = new PreviewPoint(0, 0);
					_topLeft.PointType = PreviewPoint.PointTypes.SizeTopLeft;
				}
				Point p = new Point(_topLeft.X, _topLeft.Y);
				return p;
			}
			set
			{
				_topLeft.X = value.X;
				_topLeft.Y = value.Y;
				_topRight.Y = value.Y;
				_bottomLeft.X = value.X;
				PreviewTools.TransformPreviewPoint(this);
				Layout();
			}
		}

		[CategoryAttribute("Position"),
		 DisplayName("Bottom Right"),
		 DescriptionAttribute("An arch is defined by a 2 points of a rectangle. This is point 2.")]
		public Point BottomRight
		{
			get
			{
				if (_bottomRight == null)
				{
					_bottomRight = new PreviewPoint(0, 0);
					_bottomRight.PointType = PreviewPoint.PointTypes.SizeBottomRight;
				}
				Point p = new Point(_bottomRight.X, _bottomRight.Y);
				return p;
			}
			set
			{
				_bottomRight.X = value.X;
				_bottomRight.Y = value.Y;
				_topRight.X = value.X;
				_bottomLeft.Y = value.Y;
				PreviewTools.TransformPreviewPoint(this);
				Layout();
			}
		}

		[CategoryAttribute("Size"),
		 DisplayName("Width"),
		 DescriptionAttribute("An arch is defined by a 2 points of a rectangle. This is the width of those points.")]
		public int Width
		{
			get
			{
				return Math.Abs(_bottomRight.X -_topLeft.X);
			}
			set
			{
				_bottomRight.X = _topLeft.X + value;
				_topRight.X = _bottomRight.X;
				Layout();
			}
		}

		[CategoryAttribute("Size"),
		 DisplayName("Height"),
		 DescriptionAttribute("An arch is defined by a 2 points of a rectangle. This is the height of those points.")]
		public int Height
		{
			get
			{
				return Math.Abs(_bottomRight.Y - _topLeft.Y);
			}
			set
			{
				_topLeft.Y = _bottomRight.Y - value;
				_topRight.Y = _topLeft.Y;
				Layout();
			}
		}

		[CategoryAttribute("Settings"),
		 DisplayName("Light Count"),
		 DescriptionAttribute("Number of pixels or lights in the arch.")]
		public int PixelCount
		{
			get { return Pixels == null ? 0 : Pixels.Count; }
			set
			{
				while (Pixels.Count > value)
				{
					Pixels.RemoveAt(Pixels.Count - 1);
				}
				while (Pixels.Count < value)
				{
					PreviewPixel pixel = new PreviewPixel(10, 10, 0, PixelSize);
					Pixels.Add(pixel);
				}
				Layout();
			}
		}


        public override int Top
        {
            get
            {
                return Math.Min(_topLeft.Y, _bottomRight.Y);
            }
            set
            {
                int delta = Top - value;

                _topLeft.Y -= delta;
                _bottomRight.Y -= delta;
                Layout();
            }
        }

        public override int Bottom
        {
            get
            {
                return Math.Max(_topLeft.Y, _bottomRight.Y);
            }
        }

        public override int Left
        {
            get
            {
                return Math.Min(_topLeft.X, _bottomRight.X);
            }
            set
            {
                int delta = Left - value;
                _topLeft.X -= delta;
                _bottomRight.X -= delta;
                Layout();
            }
        }

        [Browsable(false)]
        public override int Right
        {
            get
            {
                return Math.Max(_topLeft.X, _bottomRight.X);
            }
        }

        public override void Match(PreviewBaseShape matchShape)
        {
            PreviewArch shape = (matchShape as PreviewArch);
            Width = shape.Width;
            Height = shape.Height;
            PixelSize = shape.PixelSize;
			base.Match(shape);
        }

		public override void Layout()
		{
			if (_pixels != null)
			{
				int width = BottomRight.X - TopLeft.X;
				int height = BottomRight.Y - TopLeft.Y;
				var points = PreviewTools.GetArcPoints(width, height, PixelCount);
				int pointNum = 0;
				foreach (PreviewPixel pixel in _pixels)
				{
					pixel.X = points[pointNum].X + TopLeft.X;
					pixel.Y = points[pointNum].Y + TopLeft.Y;
					pointNum++;
				}

				SetPixelZoomRotate();
			}
		}
		public override void MouseMove(int x, int y, int changeX, int changeY)
		{
			// See if we're resizing
			if (_selectedPoint != null)
			{
				var point = PreviewTools.TransformPreviewPoint(this, new PreviewPoint(x, y), -ZoomLevel, PreviewTools.RotateTypes.Counterclockwise);
				if (_selectedPoint.PointType == PreviewPoint.PointTypes.SizeTopRight)
				{
					_topLeft.Y = point.Y;
					_bottomRight.X = point.X;
				}
				else if (_selectedPoint.PointType == PreviewPoint.PointTypes.SizeBottomLeft)
				{
					_topLeft.X = point.X;
					_bottomRight.Y = point.Y;
				}
				_selectedPoint.X = point.X;
				_selectedPoint.Y = point.Y;
			}
			// If we get here, we're moving
			else
			{
				changeX = Convert.ToInt32(p1Start.X + changeX/ZoomLevel) - _topLeft.X;
				changeY = Convert.ToInt32(p1Start.Y + changeY/ZoomLevel) - _topLeft.Y;

				_topLeft.X += changeX;
				_topLeft.Y += changeY;
				_bottomRight.X += changeX;
				_bottomRight.Y += changeY;
			}

			TopRight.X = _bottomRight.X;
			TopRight.Y = _topLeft.Y;
			BottomLeft.X = _topLeft.X;
			BottomLeft.Y = _bottomRight.Y;
			base.MouseMove(x, y, changeX, changeY);
			Layout();
		}

		public override void SelectDragPoints()
		{
			List<PreviewPoint> points = new List<PreviewPoint>();
			points.Add(_topLeft);
			points.Add(_bottomRight);
			points.Add(TopRight);
			points.Add(BottomLeft);
			SetSelectPoints(points, null);
		}

		public override bool PointInShape(PreviewPoint point)
		{
			foreach (PreviewPixel pixel in Pixels)
			{
				Rectangle r = new Rectangle(pixel.X - (SelectPointSize / 2), pixel.Y - (SelectPointSize / 2), SelectPointSize,
				                            SelectPointSize);
				if (point.X >= r.X && point.X <= r.X + r.Width && point.Y >= r.Y && point.Y <= r.Y + r.Height)
				{
					return true;
				}
			}
			return false;
		}

		public override void SetSelectPoint(PreviewPoint point)
		{
			if (point == null || (point.X == 0 && point.Y == 0))
			{
				p1Start = new PreviewPoint(_topLeft.X, _topLeft.Y);
				p2Start = new PreviewPoint(_bottomRight.X, _bottomRight.Y);
			}

			_selectedPoint = point;
			base.SetSelectPoint(point);
		}

		public override void SelectDefaultSelectPoint()
		{
			_selectedPoint = _bottomRight;
		}

		public override object Clone()
		{
			var newArch =(PreviewArch)MemberwiseClone();
			newArch._topLeft = _topLeft.Copy();
			newArch._bottomRight = _bottomRight.Copy();
			newArch.Pixels = new List<PreviewPixel>();
			foreach (var previewPixel in Pixels)
			{
				newArch.Pixels.Add(previewPixel.Clone());
			}

			return newArch;
		}

		public override void MoveTo(int x, int y)
		{
			Point topLeft = new Point();
			topLeft.X = Math.Min(TopLeft.X, BottomRight.X);
			topLeft.Y = Math.Min(TopLeft.Y, BottomRight.Y);

			int deltaX = x - topLeft.X;
			int deltaY = y - topLeft.Y;

            TopLeft = new Point(TopLeft.X + deltaX, TopLeft.Y + deltaY);
            BottomRight = new Point(BottomRight.X + deltaX, BottomRight.Y + deltaY);

            if (TopRight != null)
            {
                TopRight.X = _bottomRight.X;
                TopRight.Y = _topLeft.Y;
                BottomLeft.X = _topLeft.X;
                BottomLeft.Y = _bottomRight.Y;
            }

			Layout();
		}

		public override void Resize(double aspect)
		{
			TopLeft = new Point((int) (TopLeft.X*aspect), (int) (TopLeft.Y*aspect));
			BottomRight = new Point((int) (BottomRight.X*aspect), (int) (BottomRight.Y*aspect));
			Layout();
		}

		public override void ResizeFromOriginal(double aspect)
		{
			_topLeft.X = p1Start.X;
			_topLeft.Y = p1Start.Y;
			_bottomRight.X = p2Start.X;
			_bottomRight.Y = p2Start.Y;
			Resize(aspect);
		}
	}
}