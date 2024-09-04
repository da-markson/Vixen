﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Controls.WpfPropertyGrid;
using System.Windows.Media;
using Catel.Data;
using Catel.Logging;
using Catel.MVVM;
using Newtonsoft.Json.Linq;
using VixenModules.App.CustomPropEditor.Extensions;
using VixenModules.App.CustomPropEditor.Model;
using VixenModules.App.CustomPropEditor.Services;
using PropertyData = Catel.Data.PropertyData;

namespace VixenModules.App.CustomPropEditor.ViewModels
{
	[DisplayName("Element Model")]
	public sealed class ElementModelViewModel : ViewModelBase, ISelectableExpander, IDisposable
	{
		private DateTime _selectedTime = DateTime.MaxValue;
		private string _textHoldValue = String.Empty;
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();

		public ElementModelViewModel(ElementModel model, ElementModelViewModel parent)
		{
			ElementModel = model;
			ChildrenViewModels = new ElementViewModelCollection(model.Children, this);
			ElementModelLookUpService.Instance.AddModel(model.Id, this);
			((IRelationalViewModel)this).SetParentViewModel(parent);
			DeferValidationUntilFirstSaveCall = false;
			AlwaysInvokeNotifyChanged = true;
			
		}

		#region ElementModel model property

		/// <summary>
		/// Gets or sets the ElementModel value.
		/// </summary>
		[Browsable(false)]
		[Model]
		public ElementModel ElementModel
		{
			get { return GetValue<ElementModel>(ElementModelProperty); }
			private set { SetValue(ElementModelProperty, value); }
		}

		/// <summary>
		/// ElementModel property data.
		/// </summary>
		public static readonly IPropertyData ElementModelProperty = RegisterProperty<ElementModel>(nameof(ElementModel));

		#endregion

		#region Children property

		/// <summary>
		/// Gets or sets the Children value.
		/// </summary>
		[Browsable(false)]
		[ViewModelToModel("ElementModel")]
		public ObservableCollection<ElementModel> Children
		{
			get { return GetValue<ObservableCollection<ElementModel>>(ChildrenProperty); }
			set { SetValue(ChildrenProperty, value); }
		}

		/// <summary>
		/// Children property data.
		/// </summary>
		public static readonly IPropertyData ChildrenProperty = RegisterProperty<ObservableCollection<ElementModel>>(nameof(Children));

		#endregion

		#region ChildrenViewModels property

		/// <summary>
		/// Gets or sets the ChildrenViewModels value.
		/// </summary>
		[Browsable(false)]
		public ElementViewModelCollection ChildrenViewModels
		{
			get { return GetValue<ElementViewModelCollection>(ChildrenViewModelsProperty); }
			set { SetValue(ChildrenViewModelsProperty, value); }
		}

		/// <summary>
		/// ChildrenViewModels property data.
		/// </summary>
		public static readonly IPropertyData ChildrenViewModelsProperty = RegisterProperty<ElementViewModelCollection>(nameof(ChildrenViewModels));

		#endregion

		#region IsSelected property

		/// <summary>
		/// Gets or sets the IsSelected value.
		/// </summary>
		[Browsable(false)]
		public bool IsSelected
		{
			get
			{
				return GetValue<bool>(IsSelectedProperty);
			}
			set
			{
				bool tempDirty = IsDirty;
				SetValue(IsSelectedProperty, value);
				_selectedTime = DateTime.Now;
				IsDirty = tempDirty;
			}
		}

		/// <summary>
		/// IsSelected property data.
		/// </summary>
		public static readonly IPropertyData IsSelectedProperty = RegisterProperty<bool>(nameof(IsSelected));

		#endregion

		#region IsExpanded property

		/// <summary>
		/// Gets or sets the IsExpanded value.
		/// </summary>
		[Browsable(false)]
		public bool IsExpanded
		{
			get
			{
				return GetValue<bool>(IsExpandedProperty);
			}
			set
			{
				bool tempDirty = IsDirty;
				SetValue(IsExpandedProperty, value);
				IsDirty = tempDirty;
			}
		}

		/// <summary>
		/// IsExpanded property data.
		/// </summary>
		public static readonly IPropertyData IsExpandedProperty = RegisterProperty<bool>(nameof(IsExpanded));

		#endregion

		#region IsEditing property

		/// <summary>
		/// Gets or sets the IsEditing value.
		/// </summary>
		[Browsable(false)]
		public bool IsEditing
		{
			get { return GetValue<bool>(IsEditingProperty); }
			set { SetValue(IsEditingProperty, value); }
		}

		/// <summary>
		/// IsEditing property data.
		/// </summary>
		public static readonly IPropertyData IsEditingProperty = RegisterProperty<bool>(nameof(IsEditing));

		#endregion

		#region IsLeaf property

		/// <summary>
		/// Gets or sets the IsLeaf value.
		/// </summary>
		[Browsable(false)]
		public bool IsLeaf => ElementModel.IsLeaf;

		#endregion

		#region IsLightNode property

		/// <summary>
		/// Gets or sets the IsLightNode value.
		/// </summary>
		[Browsable(false)]
		public bool IsLightNode => ElementModel.IsLightNode;

		#endregion

		#region Name Property

		[PropertyOrder(0)]
		public string Name
		{
			get { return ElementModel.Name; }
			set
			{
				object oldValue = ElementModel.Name;
				ElementModel.Name = value;
				IsDirty = true;
				RaisePropertyChanged(nameof(Name));
			}
		}

		#endregion

		#region FaceComponent property

		/// <summary>
		/// Gets or sets the FaceComponent value.
		/// </summary>
		[PropertyOrder(1)]
		[DisplayName("Face Component")]
		[Description("Face component associated with this element for Lip-Sync.")]
		public FaceComponent FaceComponent
		{
			get => ElementModel.FaceDefinition.FaceComponent;
			set {
				
				object oldValue = ElementModel.FaceDefinition.FaceComponent;
				ElementModel.FaceDefinition.FaceComponent = value;
				IsDirty = true;
				RaisePropertyChanged(nameof(FaceComponent)); 
			}
		}

		#endregion

		#region FaceColor property

		/// <summary>
		/// Gets or sets the FaceColor value.
		/// </summary>
		[PropertyOrder(2)]
		[DisplayName("Face Color")]
		[Description("Face color in Hex associated with this element for Lip-Sync.")]
		public String FaceColor
		{
			get => ElementModel.FaceDefinition.FaceComponent != FaceComponent.None?
				ElementModel.FaceDefinition.DefaultColor.ToHex():
				String.Empty;
			set
			{

				if (ElementModel.FaceDefinition != null)
				{
					var color = HexToColor(value);

					ElementModel.FaceDefinition.DefaultColor = color;

					IsDirty = true;
					RaisePropertyChanged(nameof(FaceColor));
				}
			}
		}

		#endregion

		#region StateName property

		/// <summary>
		/// Gets or sets the StateDefinition Name value.
		/// </summary>
		[PropertyOrder(3)]
		[DisplayName("State Name")]
		[Description("Name of the State for this element.")]
		public String StateName
		{
			get => ElementModel.StateDefinition != null ? ElementModel.StateDefinition.Name:String.Empty;
			set
			{
				if (ElementModel.StateDefinition != null)
				{
					if (!string.IsNullOrEmpty(value))
					{
						ElementModel.StateDefinition.Name = value;
						IsDirty = true;
						RaisePropertyChanged(nameof(StateName));
					}
					else
					{
						ElementModel.StateDefinition = null;
						IsDirty = true;
						RaisePropertyChanged(nameof(StateColor));
						RaisePropertyChanged(nameof(StateIndex));
						RaisePropertyChanged(nameof(StateName));
					}
				}
				else
				{
					ElementModel.StateDefinition = new StateDefinition()
					{
						Name = value
					};
					IsDirty = true;
					RaisePropertyChanged(nameof(StateColor));
					RaisePropertyChanged(nameof(StateIndex));
					RaisePropertyChanged(nameof(StateName));
				}
				
			}
		}

		#endregion

		#region StateColor property

		/// <summary>
		/// Gets or sets the StateDefinition Color value.
		/// </summary>
		[PropertyOrder(4)]
		[DisplayName("State Color")]
		[Description("Color in Hex (#FFFFFF) associated with the State of this element.")]
		public String StateColor
		{
			get => ElementModel.StateDefinition != null ?
				ElementModel.StateDefinition.DefaultColor.ToHex() :
				String.Empty;
			set
			{
				var color = HexToColor(value);
				if (ElementModel.StateDefinition != null)
				{
					ElementModel.StateDefinition.DefaultColor = color;
					IsDirty = true;
					RaisePropertyChanged(nameof(StateColor));
				}
				else
				{
					ElementModel.StateDefinition = new StateDefinition()
					{
						DefaultColor = color
					};
					IsDirty = true;
					RaisePropertyChanged(nameof(StateName));
					RaisePropertyChanged(nameof(StateColor));
					RaisePropertyChanged(nameof(StateIndex));
				}

			}
		}

		#endregion

		#region StateIndex property

		/// <summary>
		/// Gets or sets the StateDefinition Index value.
		/// </summary>
		[PropertyOrder(5)]
		[DisplayName("State Index")]
		[Description("Index associated with the State of this element.")]
		public string StateIndex
		{
			get => ElementModel.StateDefinition != null ?
				ElementModel.StateDefinition.Index.ToString() :
				String.Empty;
			set
			{
				if (value.IsNumeric())
				{
					if (ElementModel.StateDefinition != null)
					{
						ElementModel.StateDefinition.Index = Convert.ToInt32(value);
						IsDirty = true;
						RaisePropertyChanged(nameof(StateIndex));
					}
				}
				
			}
		}

		#endregion


		#region ChildCount property

		/// <summary>
		/// Gets or sets the ChildCount value.
		/// </summary>
		[DisplayName("Children")]
		[Description("Number of child elements associated with this element.")]
		[PropertyOrder(6)]
		public int ChildCount => ElementModel.Children.Count();

		#endregion

		#region Light Count property

		/// <summary>
		/// Gets the Light Count value.
		/// </summary>
		[DisplayName("Lights")]
		[Description("Number of light elements under this element.")]
		[PropertyOrder(7)]
		public int LightCount => ElementModel.GetLeafEnumerator().Count(x => x.IsLightNode);

		#endregion

		#region BeginEdit command

		private Command _beginEditCommand;

		/// <summary>
		/// Gets the LeftMouseUp command.
		/// </summary>
		[Browsable(false)]
		public Command BeginEditCommand
		{
			get { return _beginEditCommand ?? (_beginEditCommand = new Command(BeginEdit)); }
		}

		/// <summary>
		/// Method to invoke when the LeftMouseUp command is executed.
		/// </summary>
		private void BeginEdit()
		{
			if (IsSelected && _selectedTime.AddMilliseconds(750) < DateTime.Now)
			{
				IsEditing = true;
				_textHoldValue = ElementModel.Name;
			}
		}

		#endregion

		#region DoneEditing command

		private Command _doneEditingCommand;

		/// <summary>
		/// Gets the EditFocusLost command.
		/// </summary>
		[Browsable(false)]
		public Command DoneEditingCommand
		{
			get { return _doneEditingCommand ?? (_doneEditingCommand = new Command(DoneEditing)); }
		}

		/// <summary>
		/// Method to invoke when the EditFocusLost command is executed.
		/// </summary>
		private void DoneEditing()
		{
			if (PropModelServices.Instance().IsNameDuplicated(ElementModel.Name))
			{
				ElementModel.Name = PropModelServices.Instance().Uniquify(ElementModel.Name);
			}
			IsEditing = false;
			IsDirty = true;
		}

		#endregion

		#region CancelEditing command

		private Command _cancelEditingCommand;

		/// <summary>
		/// Gets the CancelEditing command.
		/// </summary>
		[Browsable(false)]
		public Command CancelEditingCommand
		{
			get { return _cancelEditingCommand ?? (_cancelEditingCommand = new Command(CancelEditing)); }
		}

		/// <summary>
		/// Method to invoke when the CancelEditing command is executed.
		/// </summary>
		private void CancelEditing()
		{
			IsEditing = false;
			ElementModel.Name = _textHoldValue;
		}

		#endregion

		#region Overrides

		//We are not using these properties in the view so hiding them so the property giris does not expose them.

		[Browsable(false)]
		public new DateTime ViewModelConstructionTime => base.ViewModelConstructionTime;

		[Browsable(false)]
		public new int UniqueIdentifier => base.UniqueIdentifier;

		[Browsable(false)]
		public new string Title => base.Title;

		[Browsable(false)]
		public new bool IsClosed => base.IsClosed;

		[Browsable(false)]
		public new IViewModel ParentViewModel => base.ParentViewModel;
		
		[Browsable(false)]
		public new bool IsCanceled => base.IsCanceled;

		[Browsable(false)]
		public new bool IsSaved => base.IsSaved;

		#endregion

		public void RemoveFromParent()
		{
			var parentVm = ParentViewModel as ElementModelViewModel;
			if (parentVm != null)
			{
				PropModelServices.Instance().RemoveFromParent(ElementModel, parentVm.ElementModel);
				IsDirty = true;
			}
		}

		public IEnumerable<ElementModelViewModel> GetLeafEnumerator()
		{
			if (IsLeaf)
			{
				return (new[] { this });
			}

			return ChildrenViewModels.SelectMany(x => x.GetLeafEnumerator());
		}


		protected override void ValidateFields(List<IFieldValidationResult> validationResults)
		{
			if (string.IsNullOrEmpty(ElementModel.Name))
			{
				validationResults.Add(FieldValidationResult.CreateError(nameof(ElementModel.Name), "Name can not be empty"));
			}
			else if (PropModelServices.Instance().IsNameDuplicated(ElementModel.Name))
			{
				validationResults.Add(FieldValidationResult.CreateError(nameof(ElementModel.Name), "Duplicate name"));
			}

			if (ElementModel.LightSize <= 0)
			{
				validationResults.Add(FieldValidationResult.CreateError(nameof(ElementModel.LightSize), "Light size must be > 0"));
			}
		}

		private static bool IsValidHexColor(string hexColor)
		{
			return Regex.IsMatch(hexColor, "^#(?:[0-9a-fA-F]{3}){1,2}$");
		}

		private static System.Drawing.Color HexToColor(string hexColor)
		{
			if (!IsValidHexColor(hexColor)) return System.Drawing.Color.Empty;

			try
			{
				var mediaColor = ColorConverter.ConvertFromString(hexColor);
				if (mediaColor != null)
				{
					var color = (Color)mediaColor;
					return color.ToDrawingColor();
				}

			}
			catch (NullReferenceException e)
			{
				Logging.Error(e, $"Could not parse Hex code for Color {hexColor}. Defaulting to nothing");
			}

			return System.Drawing.Color.Empty;
		}

		public void Dispose()
		{
			((IRelationalViewModel)this).SetParentViewModel(null);
			ElementModelLookUpService.Instance.RemoveModel(ElementModel.Id, this);
		}


	}
}
