using CoreGraphics;
using Foundation;
using UIKit;

namespace UXDivers.Popups.Maui
{
    internal class PopupBackgroundView : UIView
    {
        private Thickness _basePadding = new (0, 0, 0, 0);
        private DisplayOrientation? _lastAppliedOrientation;

        private PopupPage? _popupBase;
        public PopupPage? PopupPage
        {
            get => _popupBase;
            set
            {
                _popupBase = value;
                if (_popupBase != null)
                {
                    _basePadding = _popupBase.Padding;
                }
            }
        }

        public UIView? PopupContentView { get; set; }
        public Func<Task>? BackgroundTappedAction { get; set; }
        
        public PopupBackgroundView()
        {
            InsetsLayoutMarginsFromSafeArea = false;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            UpdateLayoutAndSafeArea();
        }

        public override UIView? HitTest(CGPoint point, UIEvent? uievent)
        {
            if (uievent == null)
            {
                return null;
            }

            if (PopupContentView != null)
            {
                // Convert the touch point to the coordinate system of the popup content.
                CGPoint pointInPopup = ConvertPointToView(point, PopupContentView);

                // If the point is inside the popup content, let that view (or its subviews) handle the touch.
                if (PopupContentView.PointInside(pointInPopup, uievent))
                {
                    return PopupContentView.HitTest(pointInPopup, uievent);
                }
            }

            // if the background is transparent don't handle the touch. 
            if (PopupPage?.BackgroundInputTransparent == true)
            {
                return null;
            }

            // Otherwise, return self so that touchesEnded is called.
            return base.HitTest(point, uievent);
        }

        /// <summary>
        /// Called when touches end on this view. If the user taps outside the popup content,
        /// trigger the background tap action.
        /// </summary>
        public override void TouchesEnded(NSSet touches, UIEvent? evt)
        {
            // Ensure the touch is not inside the popup content.
            if (PopupContentView != null)
            {
                foreach (UITouch touch in touches)
                {
                    CGPoint point = touch.LocationInView(this);
                    CGPoint pointInPopup = ConvertPointToView(point, PopupContentView);
                    if (PopupContentView.PointInside(pointInPopup, evt))
                    {
                        // Touch was on the popup; do not trigger dismissal.
                        base.TouchesEnded(touches, evt);
                        return;
                    }
                }
            }

            // The touch is considered to be on the background.
            BackgroundTappedAction?.Invoke();
            base.TouchesEnded(touches, evt);
        }

        private void UpdateLayoutAndSafeArea()
        {
            if (PopupPage is null)
            {
                return;
            }

            var currentOrientation = DeviceDisplay.Current.MainDisplayInfo.Orientation;
            
            if (_lastAppliedOrientation == currentOrientation)
            {
                return;
            }

            _lastAppliedOrientation = currentOrientation;
            
            var safeAreaInsets = SafeAreaInsets;
            var popupSafeAreaInsets = PopupPage.SafeAreaAsPadding;
            
            var topInset = popupSafeAreaInsets.HasFlag(SafeAreaAsPadding.Top) ? safeAreaInsets.Top : 0d;
            var leftInset = popupSafeAreaInsets.HasFlag(SafeAreaAsPadding.Left) ? safeAreaInsets.Left : 0d;
            var rightInset = popupSafeAreaInsets.HasFlag(SafeAreaAsPadding.Right) ? safeAreaInsets.Right : 0d;
            var bottomInset = popupSafeAreaInsets.HasFlag(SafeAreaAsPadding.Bottom) ? safeAreaInsets.Bottom : 0d;

            PopupPage.Padding = new Thickness(
                _basePadding.Left + leftInset,
                _basePadding.Top + topInset,
                _basePadding.Right + rightInset,
                _basePadding.Bottom + bottomInset
            );

            PopupPage.Arrange(new Rect(0, 0, Bounds.Width, Bounds.Height));
        }
    }
}