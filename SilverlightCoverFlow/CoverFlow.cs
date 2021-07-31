using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Windows.Threading;

namespace SilverlightCoverFlow
{
    public static class Extensions
    {
        /// <summary>
        /// Eases the current value to the target value by a factor of <paramref name="easingFactor"/>
        /// </summary>
        /// <param name="currentValue"></param>
        /// <param name="targetValue"></param>
        /// <param name="easingFactor"></param>
        public static T GetTransform<T>(this Transform transform)
            where T : Transform
        {
            if (!(transform is TransformGroup))
                return null;

            var transformGroup = transform as TransformGroup;
            T childTransform = transformGroup.Children.FirstOrDefault(t => t is T) as T;
            return childTransform;
        }
    }

    public class CoverFlow : UserControl
    {
        public class UiElementEventArgs : EventArgs
        {
            public UIElement UiElement { get; set; }

            public UiElementEventArgs(UIElement uiElement)
            {
                this.UiElement = uiElement;
            }
        }

        public event EventHandler<UiElementEventArgs> SelectedItemChanged;
        public event EventHandler<UiElementEventArgs> ItemChangeFinished;


        #region Private fields

        /// <summary>
        /// A dictionary that contains a unique ID for each child
        /// </summary>
        private Dictionary<int, UIElement> childrenIndex = new Dictionary<int, UIElement>();

        /// <summary>
        /// Represents the index of the selected child
        /// </summary>
        public int selectedChild = -1;

        #endregion

        #region Properties

        /// <summary>
        /// Gets and sets the UseAnimations property
        /// </summary>
        public bool UseAnimations { get; set; }
        public bool UseSkewing { get; set; }
        public bool UseScaling { get; set; }
        public bool UseOpacity { get; set; }

        public int MainCoverMargin { get; set; }
        public int CoverSpacing { get; set; }

        public double PositionDecelleration { get; set; }
        public double SkewDecelleration { get; set; }
        public double ScaleDecelleration { get; set; }
        public double OpacityDecelleration { get; set; }
        public int MaxVisible { get; set; }

        public double MaxSkew { get; set; }
        public double MinScaling { get; set; }
        public double MaxScaling { get; set; }
        public double MinOpacity { get; set; }
        public double OpacityDecrement { get; set; }
        public double ScalingDecrement { get; set; }

        #endregion


        #region Initialization

        /// <summary>
        /// Initializes the control
        /// </summary>
        public CoverFlow()
        {
            // Load
            this.Loaded += new System.Windows.RoutedEventHandler(CoverFlow_Loaded);
            this.IsHitTestVisible = true;
            this.IsTabStop = true;

            this.Content = this.layout;
        }

        Grid layout = new Grid();

        /// <summary>
        /// Loads the control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CoverFlow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {

            // Initialize the default parameters
            this.InitializeDefaultParameters();

            // Populate children
            this.PopulateChildren();

            // Check for animations and disable temp
            bool isUsingAnimations = this.UseAnimations;
            this.UseAnimations = false;

            // Transform the children
            this.TransformChildren();
            this.UseAnimations = isUsingAnimations;

   

            // Select the first component
            this.First();
        }

        /// <summary>
        /// Initializes the default parameters if they aren't set
        /// </summary>
        private void InitializeDefaultParameters()
        {
            this.timer.Interval = TimeSpan.FromMilliseconds(1000 / 120);
            timer.Tick += new EventHandler(timer_Tick);

            this.UseAnimations = true;
            this.UseSkewing = true;
            this.UseScaling = true;
            this.UseOpacity = true;

            this.PositionDecelleration = 10;
            this.SkewDecelleration = 5;
            this.ScaleDecelleration = 5;
            this.OpacityDecelleration = 5;

            this.MaxSkew = 50;

            this.MinScaling = 0.1;
            this.MaxScaling = 0.8;
            this.ScalingDecrement = 0.03;
            
            this.MinOpacity = 0;
            this.OpacityDecrement = 0.7;

            this.CoverSpacing = 50;
            this.MainCoverMargin = 40;
            this.MaxVisible = 10;

            //public bool UseAnimations { get; set; }
            //public bool UseSkewing{ get; set; }
            //public bool UseScaling { get; set; }
            //public bool UseOpacity { get; set; }

            //public double PositionDecelleration { get; set; }
            //public double SkewDecelleration { get; set; }
            //public double ScaleDecelleration { get; set; }
            //public double OpacityDecelleration { get; set; }

            //public double MaxSkew { get; set; }
            //public double MinScaling { get; set; }
            //public double MinOpacity { get; set; }
        }

        
        private List<UIElement> originalChildren;

        /// <summary>
        /// Populates all children of the control into a dictionary
        /// </summary>
        private void PopulateChildren()
        {
            // Loop through all the children and create ID's for them
            int totalChildren;
            this.originalChildren = new List<UIElement>();
            if ((totalChildren = this.layout.Children.Count) > 0)
            {
                // Reset the index
                this.childrenIndex.Clear();

                // for
                foreach (UIElement child in this.layout.Children)
                    originalChildren.Add(child);

                // Clear 
                this.layout.Children.Clear();

                // Add the child with its ID
                for (int i = 0; i < totalChildren; i++)
                {
                    //  var p = new Planerator(originalChildren[i] as FrameworkElement);
                    //  p.MouseLeftButtonUp += p_MouseLeftButtonUp;

                    // Add the child
                    this.childrenIndex.Add(i, originalChildren[i]);
                    this.layout.Children.Add(originalChildren[i]);

                    // Make sure the rendertransform origin is in the center of the control so it scales nicely
                    this.layout.Children[i].RenderTransformOrigin = new Point(0.5f, 0.5f);
                }

            }
        }

        void p_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            int index = this.layout.Children.IndexOf(sender as UIElement);
            this.SelectChild(index);
        }


        #endregion

        #region Animation

        /// <summary>
        /// Eventhandler for the rendering event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            // Transform the children
            this.TransformChildren();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            this.TransformChildren();
            
        }


        /// <summary>
        /// Transforms all the children
        /// </summary>
        private void TransformChildren()
        {
            // Find the number of items
            int childrenCount = this.childrenIndex.Count;

            // Loop through all the children
            foreach (var dictionaryItem in this.childrenIndex)
            {
                // Find the ID and element
                var id = dictionaryItem.Key;
                var childElement = dictionaryItem.Value;

                // Transform the child
                this.TransformChild(childElement, id);
            }
        }

        /// <summary>
        /// Transforms a given child
        /// </summary>
        /// <param name="child"></param>
        private void TransformChild(UIElement child, int childIndex)
        {
            // Determine childIndex offset
            int childIndexOffset = 0;

            // Calculate the offset from the selected child
            if (childIndex != this.selectedChild)
                childIndexOffset = childIndex - this.selectedChild;

            if (MaxVisible > 0)
            {
                //bool isVisible = Math.Abs(childIndexOffset) < this.MaxVisible + 2;
                //child.Visibility = isVisible
                //    ? Visibility.Visible
                //    : Visibility.Collapsed;
            }

            // Targets for properties
            int targetZIndex = 0;
            double targetOffsetX = 0;
            double targetRotationY = 0;
            double targetScale = 1;
            double targetOpacity = 1;

            // Do this for every item that isn't selected
            if (childIndexOffset != 0)
            {
                targetScale = Math.Max(this.MinScaling, this.MaxScaling - (this.ScalingDecrement * Math.Abs(childIndexOffset)));

                targetOffsetX = childIndexOffset > 0
                    ? (this.CoverSpacing * childIndexOffset) + this.MainCoverMargin
                    : (this.CoverSpacing * childIndexOffset) - this.MainCoverMargin;


                targetZIndex = childIndexOffset > 0
                    ? childIndexOffset * -1
                    : childIndexOffset;

                targetRotationY = childIndexOffset > 0
                    ? this.MaxSkew
                    : -this.MaxSkew;

                if (Math.Abs(childIndexOffset) == 1)
                    targetRotationY *= 0.5;
                
                if (this.MaxVisible > 0)
                {
                    int absChildIndexOffset = Math.Abs(childIndexOffset);
                    if (absChildIndexOffset > this.MaxVisible)
                    {
                        targetOpacity = 1 - (this.OpacityDecrement * (absChildIndexOffset - this.MaxVisible));
                        //targetOffsetX = 0;
                    }
                }
            }

            // currents
            double currentPositionX = 0;
            double currentRotationY = 0;
            double currentScale = 1;
            double currentOpacity = 1;
            double newPositionX = 0;
            double newRotationY = 0;
            double newScale = 1;
            double newOpacity = 1;

            // Find the UI element
            var childElement = child as UIElement;
            if (childElement.RenderTransform != null)
            {
                var translateTransform = childElement.RenderTransform.GetTransform<TranslateTransform>();
                var scaleTransform = childElement.RenderTransform.GetTransform<ScaleTransform>();

                if (translateTransform != null)
                    currentPositionX = translateTransform.X;

                if (childElement.Projection != null)    
                    currentRotationY = (childElement.Projection as PlaneProjection).RotationY;
                
                if (scaleTransform != null)
                    currentScale = scaleTransform.ScaleX;

                currentOpacity = childElement.Opacity;
            }

            if (this.UseAnimations)
            {
                // Calculate the new position and scale
                newPositionX = currentPositionX + (targetOffsetX - currentPositionX) / this.PositionDecelleration;
                newRotationY = currentRotationY + (targetRotationY - currentRotationY) / this.SkewDecelleration;
                newScale = currentScale + (targetScale - currentScale) / this.ScaleDecelleration;
                newOpacity = currentOpacity + (targetOpacity - currentOpacity) / this.OpacityDecelleration;

                // Calculate the delta for the bounce
                // segmentDelta = (targetOffsetX - currentPositionX) * 0.2f + segmentDelta * 0.7f;

                // Add a portion of the difference to the offset
                // newPositionX = currentPositionX + segmentDelta;
            }
            else
            {
                newPositionX = targetOffsetX;
                newRotationY = targetRotationY;
                newScale = targetScale;
                newOpacity = targetOpacity;
            }

            // 
            TransformGroup elementTransforms = new TransformGroup();


            // Set the transform on the child
            elementTransforms.Children.Add(new ScaleTransform()
            {
                ScaleX = newScale,
                ScaleY = newScale
            });

            // Set the transform on the child
            elementTransforms.Children.Add(new TranslateTransform()
            {
                X = newPositionX
            });




            childElement.Projection = new PlaneProjection()
            {
                RotationX = 0,
                RotationY = newRotationY,
                RotationZ = 0,

                LocalOffsetX = 0,
                LocalOffsetY = 0,
                LocalOffsetZ = 0

            };

            // Apply transforms
            childElement.RenderTransform = elementTransforms;

            // Set the RotationY
       //     childElement.RotationY = newRotationY;

            // Set the Z-index
            childElement.SetValue(Canvas.ZIndexProperty, (int)targetZIndex);

            // Target Opacity
            childElement.Opacity = Math.Max(this.MinOpacity, newOpacity);

            //
            if (childIndexOffset == 0 && !this.finishedSelection)
            {
                if (Math.Round(targetOffsetX) == Math.Round(currentPositionX) &&
                    Math.Round(targetOpacity) == Math.Round(currentOpacity))
                {
                    this.OnItemChangeFinished();
                    this.PauseAnimations();
                }
            }
        }

        private void OnItemChangeFinished()
        {
            if (this.ItemChangeFinished != null)
                this.ItemChangeFinished(this, null);
        }

        private double segmentDelta = 0;
        #endregion

        private bool finishedSelection = false;
        private void SelectChild(int index)
        {
            if (selectedChild == index)
                return;

            this.selectedChild = index;
            this.finishedSelection = false;
            this.OnSelectedItemChanged(this.originalChildren[this.selectedChild]);

            if (this.UseAnimations)
                this.ResumeAnimations();
        }

        public void First()
        {
            if (this.layout.Children.Count > 0)
            {
                this.SelectChild(Math.Max(0, this.selectedChild - 25));
            }
        }

        public void Prev()
        {
            var index = Math.Max(this.selectedChild - 1, 0);
            this.SelectChild(index);
        }

        public void Next()
        {
            var index = Math.Min(this.selectedChild + 1, this.childrenIndex.Count - 1);
            this.SelectChild(index);
        }

        public void NextOrFirst()
        {
            var index = (this.selectedChild + 1 >= this.childrenIndex.Count)
                ? 0
                : this.selectedChild + 1;

            this.SelectChild(index);
        }

        public void Last()
        {
            this.SelectChild(Math.Min(this.childrenIndex.Count - 1, this.selectedChild + 30));
        }

        public void SelectItem(int selectedItem)
        {
            if (selectedItem < 0)
                throw new ArgumentException("SelectedItem cannot be below zero. ");

            this.SelectChild(Math.Min(selectedItem, this.childrenIndex.Count));
        }

        protected void OnSelectedItemChanged(UIElement uiElement)
        {
            if (this.SelectedItemChanged != null)
                this.SelectedItemChanged(this, new UiElementEventArgs(uiElement));
        }

        private bool isAnimating = false;

        DispatcherTimer timer = new DispatcherTimer();

        public void ResumeAnimations()
        {
            if (!this.isAnimating)
            {
                //CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
                timer.Start();        
                this.isAnimating = true;
            }
        }
        public void PauseAnimations()
        {
            if (this.isAnimating)
            {
                //CompositionTarget.Rendering -= new EventHandler(CompositionTarget_Rendering);
               // timer.Stop();
                this.isAnimating = false;
            }
        }

        public void AddChild(FrameworkElement element)
        {
            this.layout.Children.Add(element);
        }
    }
}
