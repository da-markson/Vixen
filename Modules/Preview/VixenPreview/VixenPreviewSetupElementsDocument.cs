﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Common.Controls;
using Common.Controls.Scaling;
using Common.Controls.Theme;
using Common.Resources;
using Common.Resources.Properties;
using WeifenLuo.WinFormsUI.Docking;
using Vixen.Data.Flow;
using Vixen.Module;
using Vixen.Module.OutputFilter;
using Vixen.Rule;
using Vixen.Services;
using Vixen.Sys;
using Vixen.Sys.Output;

namespace VixenModules.Preview.VixenPreview
{
	public partial class VixenPreviewSetupElementsDocument : DockContent
	{
		private VixenPreviewControl _preview;

		public VixenPreviewSetupElementsDocument(VixenPreviewControl preview)
		{
			InitializeComponent();
			int iconSize = (int)(24 * ScalingTools.GetScaleFactor());
			buttonAddTemplate.Image = Tools.GetIcon(Resources.add, iconSize);
			buttonAddTemplate.Text = "";
			var elementTemplates = ApplicationServices.GetAllElementTemplates();
			comboBoxNewItemType.BeginUpdate();
			foreach (IElementTemplate template in elementTemplates)
			{
				ComboBoxItem item = new ComboBoxItem(template.TemplateName, template);
				comboBoxNewItemType.Items.Add(item);
			}
			comboBoxNewItemType.EndUpdate();
			if (comboBoxNewItemType.Items.Count > 0)
				comboBoxNewItemType.SelectedIndex = 0;

			ThemeUpdateControls.UpdateControls(this);
			_preview = preview;
			treeElements.AllowPropertyEdit = false;
			treeElements.AllowWireExport = false;
		}

		private void VixenPreviewSetupElementsDocument_Load(object sender, EventArgs e)
		{
			treeElements.treeviewAfterSelect += treeElements_AfterSelect;
			treeElements.treeviewDeselected += TreeElementsOnTreeviewDeselected;
			
		}

		private void HighlightNode(ElementNode node)
		{
			// Is this a group?
			if (!node.IsLeaf)
			{
				// If so, iterate through children and highlight them
				foreach (var childNode in node.Children)
				{
					HighlightNode(childNode);
				}
			}

			// Finally, highlight the node passed to us
			_preview.HighlightedElements.Add(node.Id);
			_preview.DeSelectSelectedDisplayItem();
		}

		private void treeElements_AfterSelect(object sender, TreeViewEventArgs e)
		{
			_preview.HighlightedElements.Clear();

			foreach (var node in treeElements.SelectedElementNodes) {
				HighlightNode(node);
			}

			if (treeElements.SelectedElementNodes.Count() == 1)
			{
				if (_preview.NodeToPixel.TryGetValue(treeElements.SelectedNode.GetLeafEnumerator().First(), out var previewPixel))
				{
					if (previewPixel.Any())
					{
						var shape = _preview.DisplayItemAtPoint(previewPixel.First().Point);
						if (shape != null)
						{
							_preview.propertiesForm.ShowSetupControl(shape.Shape.GetSetupControl());
						}
					}

				}
				else
				{
					if (!_preview.SelectedDisplayItems.Any())
					{
						_preview.propertiesForm.ClearSetupControl();
					}
				}
			}
		}

		private void TreeElementsOnTreeviewDeselected(object sender, EventArgs e)
		{
			_preview.HighlightedElements.Clear();
			if (!_preview.SelectedDisplayItems.Any())
			{
				_preview.propertiesForm.ClearSetupControl();
			}
		}

		public ElementNode SelectedNode => treeElements.SelectedNode;

		private void ButtonAddTemplate_Click(object sender, EventArgs e)
		{
			ComboBoxItem item = (comboBoxNewItemType.SelectedItem as ComboBoxItem);

			if (item != null)
			{
				IElementTemplate template = item.Value as IElementTemplate;
				bool act = template.SetupTemplate(treeElements.SelectedElementNodes);
				if (act)
				{
					IEnumerable<ElementNode> createdElements = template.GenerateElements(treeElements.SelectedElementNodes);
					if (createdElements == null || !createdElements.Any())
					{
						var messageBox = new MessageBoxForm("Could not create elements.  Ensure you use a valid name and try again.", "",MessageBoxButtons.OKCancel,SystemIcons.Error);
						messageBox.ShowDialog();
						return;
					}
					AddNodeToTree(createdElements.First());
				}
			}
		}

		internal void AddNodeToTree(ElementNode node)
		{
			if (node == null) return;
			treeElements.AddNodePathToTree(new[] { node });
			treeElements.UpdateScrollPosition();
		}

		private void ComboBoxNewItemType_SelectedIndexChanged(object sender, EventArgs e)
		{
			buttonAddTemplate.Enabled = comboBoxNewItemType.SelectedIndex >= 0;
		}

		private void ComboBoxNewItemType_DrawItem(object sender, DrawItemEventArgs e)
		{
			ThemeComboBoxRenderer.DrawItem(sender, e);
		}
	}
}