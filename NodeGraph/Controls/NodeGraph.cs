﻿using NodeGraph.CommandParameters;
using NodeGraph.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NodeGraph.Controls
{
    public class NodeGraph : MultiSelector
    {
        public Canvas Canvas { get; private set; } = null;

        public Key MoveWithKey
        {
            get => (Key)GetValue(MoveWithKeyProperty);
            set => SetValue(MoveWithKeyProperty, value);
        }
        public static readonly DependencyProperty MoveWithKeyProperty =
            DependencyProperty.Register(nameof(MoveWithKey), typeof(Key), typeof(NodeGraph), new FrameworkPropertyMetadata(Key.None));

        public MouseButton MoveWithMouse
        {
            get => (MouseButton)GetValue(MoveWithMouseProperty);
            set => SetValue(MoveWithMouseProperty, value);
        }
        public static readonly DependencyProperty MoveWithMouseProperty =
            DependencyProperty.Register(nameof(MoveWithMouse), typeof(MouseButton), typeof(NodeGraph), new FrameworkPropertyMetadata(MouseButton.Middle));

        public Key ScaleWithKey
        {
            get => (Key)GetValue(ScaleWithKeyProperty);
            set => SetValue(ScaleWithKeyProperty, value);
        }
        public static readonly DependencyProperty ScaleWithKeyProperty =
            DependencyProperty.Register(nameof(ScaleWithKey), typeof(Key), typeof(NodeGraph), new FrameworkPropertyMetadata(Key.None));

        public double Scale
        {
            get => (double)GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }
        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register(nameof(Scale), typeof(double), typeof(NodeGraph), new FrameworkPropertyMetadata(1.0));

        public double MinScale
        {
            get => (double)GetValue(MinScaleProperty);
            set => SetValue(MinScaleProperty, value);
        }
        public static readonly DependencyProperty MinScaleProperty =
            DependencyProperty.Register(nameof(MinScale), typeof(double), typeof(NodeGraph), new FrameworkPropertyMetadata(0.1));

        public double ScaleRate
        {
            get => (double)GetValue(ScaleRateProperty);
            set => SetValue(ScaleRateProperty, value);
        }
        public static readonly DependencyProperty ScaleRateProperty =
            DependencyProperty.Register(nameof(ScaleRate), typeof(double), typeof(NodeGraph), new FrameworkPropertyMetadata(0.1));

        public Point Offset
        {
            get => (Point)GetValue(OffsetProperty);
            set => SetValue(OffsetProperty, value);
        }
        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register(nameof(Offset), typeof(Point), typeof(NodeGraph), new FrameworkPropertyMetadata(new Point(0, 0), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OffsetPropertyChanged));

        public ICommand PreviewConnectCommand
        {
            get => (ICommand)GetValue(PreviewConnectCommandProperty);
            set => SetValue(PreviewConnectCommandProperty, value);
        }
        public static readonly DependencyProperty PreviewConnectCommandProperty =
            DependencyProperty.Register(nameof(PreviewConnectCommand), typeof(ICommand), typeof(NodeGraph), new FrameworkPropertyMetadata(null));

        public ICommand ConnectCommand
        {
            get => (ICommand)GetValue(ConnectCommandProperty);
            set => SetValue(ConnectCommandProperty, value);
        }
        public static readonly DependencyProperty ConnectCommandProperty =
            DependencyProperty.Register(nameof(ConnectCommand), typeof(ICommand), typeof(NodeGraph), new FrameworkPropertyMetadata(null));

        public ICommand DisconnectCommand
        {
            get => (ICommand)GetValue(DisconnectCommandProperty);
            set => SetValue(DisconnectCommandProperty, value);
        }
        public static readonly DependencyProperty DisconnectCommandProperty =
            DependencyProperty.Register(nameof(DisconnectCommand), typeof(ICommand), typeof(NodeGraph), new FrameworkPropertyMetadata(null));

        public ICommand MovedNodesCommand
        {
            get => (ICommand)GetValue(MovedNodesCommandProperty);
            set => SetValue(MovedNodesCommandProperty, value);
        }
        public static readonly DependencyProperty MovedNodesCommandProperty =
            DependencyProperty.Register(nameof(MovedNodesCommand), typeof(ICommand), typeof(NodeGraph), new FrameworkPropertyMetadata(null));

        public Style NodeLinkStyle
        {
            get => (Style)GetValue(NodeLinkStyleProperty);
            set => SetValue(NodeLinkStyleProperty, value);
        }
        public static readonly DependencyProperty NodeLinkStyleProperty =
            DependencyProperty.Register(nameof(NodeLinkStyle), typeof(Style), typeof(NodeGraph), new FrameworkPropertyMetadata(null, NodeLinkStylePropertyChanged));

        public Style GroupNodeStyle
        {
            get => (Style)GetValue(GroupNodeStyleProperty);
            set => SetValue(GroupNodeStyleProperty, value);
        }
        public static readonly DependencyProperty GroupNodeStyleProperty =
            DependencyProperty.Register(nameof(GroupNodeStyle), typeof(Style), typeof(NodeGraph), new FrameworkPropertyMetadata(null));

        public IEnumerable NodeLinks
        {
            get => (IEnumerable)GetValue(NodeLinksProperty);
            set => SetValue(NodeLinksProperty, value);
        }
        public static readonly DependencyProperty NodeLinksProperty =
            DependencyProperty.Register(nameof(NodeLinks), typeof(IEnumerable), typeof(NodeGraph), new FrameworkPropertyMetadata(null, NodeLinksPropertyChanged));

        public IEnumerable GroupNodes
        {
            get => (IEnumerable)GetValue(GroupNodesProperty);
            set => SetValue(GroupNodesProperty, value);
        }
        public static readonly DependencyProperty GroupNodesProperty =
            DependencyProperty.Register(nameof(GroupNodes), typeof(IEnumerable), typeof(NodeGraph), new FrameworkPropertyMetadata(null, GroupNodesPropertyChanged));

        public bool AllowToOverrideConnection
        {
            get => (bool)GetValue(AllowToOverrideConnectionProperty);
            set => SetValue(AllowToOverrideConnectionProperty, value);
        }
        public static readonly DependencyProperty AllowToOverrideConnectionProperty =
            DependencyProperty.Register(nameof(AllowToOverrideConnection), typeof(bool), typeof(NodeGraph), new FrameworkPropertyMetadata(false, AllowToOverrideConnectionPropertyChanged));

        ControlTemplate NodeTemplate => _NodeTemplate.Get("__NodeTemplate__");
        ResourceInstance<ControlTemplate> _NodeTemplate = new ResourceInstance<ControlTemplate>();

        ControlTemplate GroupNodeTemplate => _GroupNodeTemplate.Get("__GroupNodeTemplate__");
        ResourceInstance<ControlTemplate> _GroupNodeTemplate = new ResourceInstance<ControlTemplate>();

        Style NodeLinkAnimationStyle => _NodeLinkAnimationStyle.Get("__NodeLinkAnimationStyle__");
        ResourceInstance<Style> _NodeLinkAnimationStyle = new ResourceInstance<Style>();

        bool _IsNodeSelected = false;
        bool _IsStartDragging = false;
        bool _PressKeyToMove = false;
        bool _PressMouseToMove = false;
        bool _PressMouseToSelect = false;
        bool _IsRangeSelecting = false;

        class DraggingNodeLinkParam
        {
            public NodeLink NodeLink { get; } = null;
            public NodeConnectorContent StartConnector { get; } = null;

            public DraggingNodeLinkParam(NodeLink nodeLink, NodeConnectorContent connector)
            {
                NodeLink = nodeLink;
                StartConnector = connector;
            }
        }

        List<NodeBase> _DraggingNodes = new List<NodeBase>();
        NodeLink _ReconnectGhostNodeLink = null;
        DraggingNodeLinkParam _DraggingNodeLinkParam = null;
        Point _DragStartPointToMoveNode = new Point();
        Point _DragStartPointToMoveOffset = new Point();
        Point _CaptureOffset = new Point();

        Point _DragStartPointToSelect = new Point();
        RangeSelector _RangeSelector = new RangeSelector();

        HashSet<NodeConnectorContent> _PreviewedConnectors = new HashSet<NodeConnectorContent>();

        List<object> _DelayToBindNodeVMs = new List<object>();
        List<object> _DelayToBindNodeLinkVMs = new List<object>();
        List<object> _DelayToBindGroupNodeVMs = new List<object>();

        static void OffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var nodeGraph = d as NodeGraph;

            // need to calculate node position before calculate link absolute position.
            foreach (var obj in nodeGraph.Canvas.Children.OfType<Node>())
            {
                obj.UpdateOffset(nodeGraph.Offset);
            }

            foreach (var obj in nodeGraph.Canvas.Children.OfType<NodeLink>())
            {
                obj.UpdateOffset(nodeGraph.Offset);
            }

            foreach (var obj in nodeGraph.Canvas.Children.OfType<GroupNode>())
            {
                obj.UpdateOffset(nodeGraph.Offset);
            }
        }

        static void NodeLinkStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                var nodeGraph = d as NodeGraph;
                var nodeLinkStyle = e.NewValue as Style;

                // will occur exception cannot find name of element in this style scope if specified BasedOn that using RemoveStoryboard with BeginStoryboardName
                // because override style doesn't know Name of BeginStoryboardName.
                // so cannot find to remove element, so need to RegisterName that name and should be remove element here.
                foreach (var trigger in nodeGraph.NodeLinkAnimationStyle.Triggers)
                {
                    foreach (var beginStoryboard in trigger.EnterActions.OfType<BeginStoryboard>())
                    {
                        nodeLinkStyle.RegisterName(beginStoryboard.Name, beginStoryboard);
                    }
                }
                nodeLinkStyle.Seal();
            }
        }

        static void NodeLinksPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var nodeGraph = d as NodeGraph;

            var oldValue = e.OldValue as IEnumerable;
            var newValue = e.NewValue as IEnumerable;
            CollectionPropertyChanged<NodeLink>(
                nodeGraph,
                oldValue,
                newValue,
                nodeGraph.NodeLinkCollectionChanged,
                nodeGraph._DelayToBindNodeLinkVMs,
                nodeGraph.AddNodeLinksToCanvas);
        }

        static void GroupNodesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var nodeGraph = d as NodeGraph;

            var oldValue = e.OldValue as IEnumerable;
            var newValue = e.NewValue as IEnumerable;
            CollectionPropertyChanged<GroupNode>(
                nodeGraph,
                oldValue,
                newValue,
                nodeGraph.GroupNodeCollectionChanged,
                nodeGraph._DelayToBindGroupNodeVMs,
                nodeGraph.AddGroupNodesToCanvas);
        }

        static void CollectionPropertyChanged<T>(
            NodeGraph nodeGraph,
            IEnumerable oldValue,
            IEnumerable newValue,
            NotifyCollectionChangedEventHandler collectionChanged,
            List<object> delayToBindVMs,
            Action<object[]> addElementToCanvas) where T : UIElement, ICanvasObject
        {
            if (oldValue != null && oldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= collectionChanged;
            }
            if (newValue != null && newValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += collectionChanged;
            }

            // below process is node collection changed.
            if (nodeGraph.Canvas == null)
            {
                if (newValue != null && newValue is IEnumerable enumerable)
                {
                    delayToBindVMs.AddRange(enumerable.OfType<object>());
                }
                return;
            }

            // remove old node links
            var removeElements = nodeGraph.Canvas.Children.OfType<T>().ToArray();
            foreach (var removeElement in removeElements)
            {
                nodeGraph.Canvas.Children.Remove(removeElement);
            }

            // add new node links
            if (newValue != null)
            {
                var newEnumerable = newValue as IEnumerable;
                addElementToCanvas(newEnumerable.OfType<object>().ToArray());
            }
        }

        static void AllowToOverrideConnectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NodeInputContent.AllowToOverrideConnection = (bool)e.NewValue;
        }

        static NodeGraph()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NodeGraph), new FrameworkPropertyMetadata(typeof(NodeGraph)));
        }

        public Point GetDragNodePosition(MouseEventArgs e)
        {
            return e.GetPosition(Canvas);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Canvas = GetTemplateChild("__NodeGraphCanvas__") as Canvas;

            if (_DelayToBindNodeVMs.Count > 0)
            {
                AddNodesToCanvas(_DelayToBindNodeVMs.OfType<object>().ToArray());
                _DelayToBindNodeVMs.Clear();
            }

            if (_DelayToBindNodeLinkVMs.Count > 0)
            {
                AddNodeLinksToCanvas(_DelayToBindNodeLinkVMs.OfType<object>().ToArray());
                _DelayToBindNodeLinkVMs.Clear();
            }

            if (_DelayToBindGroupNodeVMs.Count > 0)
            {
                AddGroupNodesToCanvas(_DelayToBindGroupNodeVMs.OfType<object>().ToArray());
                _DelayToBindGroupNodeVMs.Clear();
            }
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);

            CollectionPropertyChanged<Node>(
                this,
                oldValue,
                newValue,
                NodeCollectionChanged,
                _DelayToBindNodeVMs,
                AddNodesToCanvas);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (MoveWithKey == Key.None || e.Key == MoveWithKey)
            {
                _PressKeyToMove = true;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            _PressKeyToMove = false;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            if (ScaleWithKey == Key.None || (Keyboard.GetKeyStates(ScaleWithKey) & KeyStates.Down) != 0)
            {
                Scale += e.Delta > 0 ? +ScaleRate : -ScaleRate;
                Scale = Math.Max(MinScale, Scale);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var posOnCanvas = e.GetPosition(Canvas);

            if (_DraggingNodes.Count > 0)
            {
                if (_IsStartDragging == false)
                {
                    if (_DraggingNodes[0].IsSelected)
                    {
                        foreach (var node in Canvas.Children.OfType<Node>().Where(arg => arg != _DraggingNodes[0] && arg.IsSelected))
                        {
                            node.CaptureDragStartPosition();
                            _DraggingNodes.Add(node);
                        }
                    }
                    else
                    {
                        BeginUpdateSelectedItems();

                        _DraggingNodes[0].IsSelected = true; // select first drag node.
                        UpdateSelectedItems(_DraggingNodes[0]);

                        foreach (var node in Canvas.Children.OfType<Node>())
                        {
                            if (node != _DraggingNodes[0])
                            {
                                node.IsSelected = false;
                                UpdateSelectedItems(node);
                            }
                        }
                        EndUpdateSelectedItems();
                    }

                    _IsStartDragging = true;
                }

                var current = e.GetPosition(Canvas);
                var diff = new Point(current.X - _DragStartPointToMoveNode.X, current.Y - _DragStartPointToMoveNode.Y);

                foreach (var node in _DraggingNodes)
                {
                    double x = node.DragStartPosition.X + diff.X;
                    double y = node.DragStartPosition.Y + diff.Y;
                    node.UpdatePosition(x, y);
                }
            }
            else if (_PressMouseToMove && (MoveWithKey == Key.None || _PressKeyToMove))
            {
                var x = _CaptureOffset.X + posOnCanvas.X - _DragStartPointToMoveOffset.X;
                var y = _CaptureOffset.Y + posOnCanvas.Y - _DragStartPointToMoveOffset.Y;
                Offset = new Point(x, y);
            }
            else if (_DraggingNodeLinkParam != null)
            {
                HitTestToPreviewConnector(posOnCanvas, _DraggingNodeLinkParam);

                _DraggingNodeLinkParam.NodeLink.UpdateEdgePoint(posOnCanvas.X, posOnCanvas.Y);
            }
            else if (_PressMouseToSelect)
            {
                if (_IsRangeSelecting == false)
                {
                    Canvas.Children.Add(_RangeSelector);

                    _IsRangeSelecting = true;
                    Cursor = Cursors.SizeAll;
                }

                _RangeSelector.RangeRect = new Rect(_DragStartPointToSelect, posOnCanvas);

                BeginUpdateSelectedItems();

                bool anyIntersects = false;
                foreach (var node in Canvas.Children.OfType<Node>())
                {
                    var nodeRect = new Rect(new Size(node.ActualWidth, node.ActualHeight));
                    var boundingBox = node.RenderTransform.TransformBounds(nodeRect);
                    node.IsSelected = _RangeSelector.RangeRect.IntersectsWith(boundingBox);

                    anyIntersects |= node.IsSelected;

                    UpdateSelectedItems(node);
                }
                _RangeSelector.IsIntersects = anyIntersects;

                EndUpdateSelectedItems();
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            var posOnCanvas = e.GetPosition(Canvas);

            if (e.LeftButton == MouseButtonState.Pressed && (MoveWithKey == Key.None || Keyboard.GetKeyStates(MoveWithKey) != KeyStates.Down))
            {
                // start to select by range rect.
                _PressMouseToSelect = true;
                _RangeSelector.Reset(posOnCanvas);

                _DragStartPointToSelect = posOnCanvas;
            }

            // it is able to offset only in state of not operating anything.
            if (_DraggingNodes.Count == 0 && _DraggingNodeLinkParam == null && _PressMouseToSelect == false)
            {
                UpdateMouseMoveState(e);
            }

            if (_PressMouseToMove)
            {
                _CaptureOffset = Offset;
                _DragStartPointToMoveOffset = posOnCanvas;
                Cursor = Cursors.ScrollAll;
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            UpdateMouseMoveState(e);
            if (_PressKeyToMove == false)
            {
                Cursor = Cursors.Arrow;
            }

            // not related if not released left mouse button.
            if (e.LeftButton != MouseButtonState.Released)
            {
                return;
            }

            _IsStartDragging = false;
            _PressMouseToSelect = false;

            if (_IsRangeSelecting)
            {
                Canvas.Children.Remove(_RangeSelector);
                _IsRangeSelecting = false;
            }
            else if (_IsNodeSelected == false)
            {
                // click empty area to unselect nodes.
                BeginUpdateSelectedItems();
                foreach (var node in Canvas.Children.OfType<Node>())
                {
                    node.IsSelected = false;
                    UpdateSelectedItems(node);
                }
                EndUpdateSelectedItems();
            }
            _IsNodeSelected = false;
            if (_DraggingNodes.Count > 0)
            {
                var param = new MovedNodesCommandParameter(_DraggingNodes.Select(arg => arg.Guid).ToArray());
                MovedNodesCommand?.Execute(param);
            }
            _DraggingNodes.Clear();

            var element = e.OriginalSource as FrameworkElement;

            // clicked connect or not.
            if (_DraggingNodeLinkParam != null)
            {
                bool connected = false;

                // only be able to connect input to output or output to input.
                // it will reject except above condition.
                if (element != null && element.Tag is NodeConnectorContent toEndConnector)
                {
                    var nodeLink = _DraggingNodeLinkParam.NodeLink;
                    var startConnector = _DraggingNodeLinkParam.StartConnector;

                    if (nodeLink.ToEndConnector == toEndConnector)
                    {
                        // tried to reconnect but reconnecting same with before connector.
                        _DraggingNodeLinkParam = null;
                        nodeLink.RestoreEndPoint();

                        connected = true;
                    }
                    else if (NodeConnectorContent.CanConnectEachOther(startConnector, toEndConnector))
                    {
                        NodeInputContent input = null;
                        NodeOutputContent output = null;

                        switch (toEndConnector)
                        {
                            case NodeInputContent inputConnector:
                                {
                                    input = inputConnector;
                                    output = _DraggingNodeLinkParam.StartConnector as NodeOutputContent;

                                    if (AllowToOverrideConnection && input.ConnectedCount > 0)
                                    {
                                        // it has to disconnect previous connected node link.
                                        var nodeLinks = Canvas.Children.OfType<NodeLink>().ToArray();
                                        var disconnectNodeLink = nodeLinks.First(arg => arg.InputGuid == input.Guid);
                                        DisconnectNodeLink(disconnectNodeLink);
                                    }
                                }
                                break;
                            case NodeOutputContent outputConnector:
                                {
                                    output = outputConnector;
                                    input = _DraggingNodeLinkParam.StartConnector as NodeInputContent;
                                }
                                break;
                            default:
                                throw new InvalidCastException();
                        }

                        if (_DraggingNodeLinkParam.NodeLink.IsConnecting)
                        {
                            // node link was already connected, so need to dispatch disconnect command.
                            DisconnectNodeLink(_DraggingNodeLinkParam.NodeLink);
                        }
                        else
                        {
                            // preview node link.
                            Canvas.Children.Remove(_DraggingNodeLinkParam.NodeLink);
                            _DraggingNodeLinkParam.NodeLink.Dispose();
                        }

                        var param = new ConnectCommandParameter(input.Node.Guid, input.Guid, output.Node.Guid, output.Guid);
                        ConnectCommand?.Execute(param);

                        connected = true;
                    }
                }

                if (connected == false)
                {
                    DisconnectNodeLink(_DraggingNodeLinkParam.NodeLink);
                }

                _DraggingNodeLinkParam = null;

                if (_ReconnectGhostNodeLink != null)
                {
                    Canvas.Children.Remove(_ReconnectGhostNodeLink);
                    _ReconnectGhostNodeLink = null;
                }
            }

            ClearPreviewedConnect();

            e.Handled = Offset != _CaptureOffset;
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            Cursor = Cursors.Arrow;

            _PressMouseToSelect = false;
            _PressMouseToMove = false;
            _IsStartDragging = false;
            _DraggingNodeLinkParam = null;
            _DraggingNodes.Clear();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            Cursor = Cursors.Arrow;

            _PressMouseToSelect = false;
            _PressMouseToMove = false;
            _IsStartDragging = false;
            if (_DraggingNodeLinkParam != null)
            {
                DisconnectNodeLink(_DraggingNodeLinkParam.NodeLink);
                if (_ReconnectGhostNodeLink != null)
                {
                    Canvas.Children.Remove(_ReconnectGhostNodeLink);
                    _ReconnectGhostNodeLink = null;
                }
            }
            _DraggingNodeLinkParam = null;
            _DraggingNodes.Clear();

            if (_IsRangeSelecting)
            {
                Canvas.Children.Remove(_RangeSelector);
                _IsRangeSelecting = false;
            }

            InvalidateVisual();
        }

        void UpdateMouseMoveState(MouseButtonEventArgs e)
        {
            switch (MoveWithMouse)
            {
                case MouseButton.Left:
                    if (MoveWithKey == Key.None)
                    {
                        throw new InvalidProgramException("Not allow combination that no set MoveWithKey(Key.None) and left click.");
                    }

                    if (Keyboard.GetKeyStates(MoveWithKey) == KeyStates.Down)
                    {
                        // offset center focus.
                        _PressMouseToMove = e.LeftButton == MouseButtonState.Pressed;
                    }
                    _PressMouseToMove = e.LeftButton == MouseButtonState.Pressed;
                    break;
                case MouseButton.Middle:
                    _PressMouseToMove = e.MiddleButton == MouseButtonState.Pressed;
                    break;
                case MouseButton.Right:
                    _PressMouseToMove = e.RightButton == MouseButtonState.Pressed;
                    break;
            }
        }

        void DisconnectNodeLink(NodeLink nodeLink)
        {
            if (nodeLink.Input != null && nodeLink.Output != null)
            {
                var inputNode = nodeLink.Input.Node;
                var outputNode = nodeLink.Output.Node;
                var param = new DisconnectCommandParameter(nodeLink.Guid, inputNode.Guid, nodeLink.InputGuid, outputNode.Guid, nodeLink.OutputGuid);
                DisconnectCommand?.Execute(param);
            }

            Canvas.Children.Remove(nodeLink);
            nodeLink.Dispose();
        }

        void ClearPreviewedConnect()
        {
            foreach (var connector in _PreviewedConnectors)
            {
                connector.CanConnect = true;
            }
            _PreviewedConnectors.Clear();
        }

        void HitTestToPreviewConnector(Point pos, DraggingNodeLinkParam param)
        {
            foreach (var connector in _PreviewedConnectors)
            {
                connector.CanConnect = true;
            }
            _PreviewedConnectors.Clear();

            Cursor = Cursors.Cross;

            VisualTreeHelper.HitTest(Canvas, null, new HitTestResultCallback(arg =>
            {
                var element = arg.VisualHit as FrameworkElement;
                if (element != null && element.Tag is NodeConnectorContent toConnector)
                {
                    if (param.StartConnector != toConnector && param.NodeLink.ToEndConnector != toConnector)
                    {
                        PreviewConnect(param.StartConnector, toConnector);
                        _PreviewedConnectors.Add(toConnector);

                        return HitTestResultBehavior.Stop;
                    }
                }
                return HitTestResultBehavior.Continue;
            }), new PointHitTestParameters(pos));
        }

        void NodeCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged<Node>(e.Action, e.OldItems, e.NewItems, RemoveNodesFromCanvas, RemoveNodesFromCanvas, AddNodesToCanvas);
        }

        void GroupNodeCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged<GroupNode>(e.Action, e.OldItems, e.NewItems, RemoveGroupNodesFromCanvas, RemoveGroupNodesFromCanvas, AddGroupNodesToCanvas);
        }

        void NodeLinkCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged<NodeLink>(e.Action, e.OldItems, e.NewItems, RemoveNodeLinksFromCanvas, RemoveNodeLinksFromCanvas, AddNodeLinksToCanvas);
        }

        void CollectionChanged<T>(
            NotifyCollectionChangedAction action,
            IList oldItems,
            IList newItems,
            Action<object[]> removeItem,
            Action<T[]> removeItemDirectly,
            Action<object[]> addItem) where T : UIElement, ICanvasObject
        {
            switch (action)
            {
                case NotifyCollectionChangedAction.Reset:
                    removeItemDirectly(Canvas.Children.OfType<T>().ToArray());
                    break;
                default:
                    if (oldItems?.Count > 0)
                    {
                        removeItem(oldItems.OfType<object>().ToArray());
                    }
                    if (newItems?.Count > 0)
                    {
                        addItem(newItems.OfType<object>().ToArray());
                    }
                    break;
            }
        }

        void RemoveNodeLinksFromCanvas(object[] removeVMs)
        {
            var removeNodeLinks = new List<NodeLink>();
            var children = Canvas.Children.OfType<NodeLink>().ToArray();

            foreach (var removeVM in removeVMs)
            {
                var removeNodeLink = children.First(arg => arg.DataContext == removeVM);
                removeNodeLinks.Add(removeNodeLink);
            }

            foreach (var removeNodeLink in removeNodeLinks)
            {
                // add node link.
                removeNodeLink.MouseDown -= NodeLink_MouseDown;
                Canvas.Children.Remove(removeNodeLink);
                removeNodeLink.Dispose();
            }
        }

        void AddNodeLinksToCanvas(object[] addVMs)
        {
            foreach (var vm in addVMs)
            {
                var nodeLink = new NodeLink(Canvas, Scale)
                {
                    DataContext = vm,
                    Style = NodeLinkStyle ?? NodeLinkAnimationStyle,
                };

                nodeLink.Validate();

                var nodeLinks = Canvas.Children.OfType<NodeLink>().ToArray();
                if (nodeLinks.Any(arg => arg.Guid == nodeLink.Guid))
                {
                    throw new InvalidOperationException($"Already exists adding node link. Guid = {nodeLink.Guid}");
                }

                var nodes = Canvas.Children.OfType<Node>().ToArray();
                var nodeInput = FindConnectorContentInNodes<NodeInputContent>(nodes, nodeLink.InputGuid);
                var nodeOutput = FindConnectorContentInNodes<NodeOutputContent>(nodes, nodeLink.OutputGuid);

                nodeInput.Connect(nodeLink);
                nodeOutput.Connect(nodeLink);

                nodeLink.Connect(nodeInput, nodeOutput);
                nodeLink.MouseDown += NodeLink_MouseDown;

                Canvas.Children.Add(nodeLink);
            }
        }

        void RemoveNodesFromCanvas(object[] removeVMs)
        {
            var removeNodes = new List<Node>();
            var children = Canvas.Children.OfType<Node>().ToArray();

            foreach (var removeVM in removeVMs)
            {
                var removeElement = children.First(arg => arg.DataContext == removeVM);
                removeNodes.Add(removeElement);
            }

            RemoveNodesFromCanvas(removeNodes.ToArray());
        }

        void RemoveNodesFromCanvas(Node[] removeNodes)
        {
            foreach (var removeNode in removeNodes)
            {
                Canvas.Children.Remove(removeNode);

                removeNode.MouseUp -= Node_MouseUp;
                removeNode.MouseDown -= Node_MouseDown;

                var nodeLinks = removeNode.EnumrateConnectedNodeLinks();
                foreach (var nodeLink in nodeLinks)
                {
                    Canvas.Children.Remove(nodeLink);
                }

                removeNode.Dispose();
            }
        }

        void AddNodesToCanvas(object[] addVMs)
        {
            foreach (var vm in addVMs)
            {
                var node = new Node(Canvas, Offset, Scale);
                node.DataContext = vm;
                node.Template = NodeTemplate;
                node.ApplyTemplate();

                node.Style = GetNodeStyle(vm, node);
                node.Initialize();

                node.MouseDown += Node_MouseDown;
                node.MouseUp += Node_MouseUp;

                Canvas.Children.Add(node);
            }
        }

        void RemoveGroupNodesFromCanvas(object[] removeVMs)
        {
            var removeGroupNodes = new List<GroupNode>();
            var children = Canvas.Children.OfType<GroupNode>().ToArray();

            foreach (var removeVM in removeVMs)
            {
                var removeElement = children.First(arg => arg.DataContext == removeVM);
                removeGroupNodes.Add(removeElement);
            }

            RemoveGroupNodesFromCanvas(removeGroupNodes.ToArray());
        }

        void RemoveGroupNodesFromCanvas(GroupNode[] removeGroupNodes)
        {
            foreach (var removeGroupNode in removeGroupNodes)
            {
                Canvas.Children.Remove(removeGroupNode);

                removeGroupNode.MouseUp -= Node_MouseDown;
                removeGroupNode.MouseDown -= GroupNode_MouseDown;

                /*
                var nodeLinks = removeGroupNode.EnumrateConnectedNodeLinks();
                foreach (var nodeLink in nodeLinks)
                {
                    Canvas.Children.Remove(nodeLink);
                }
                */

                removeGroupNode.Dispose();
            }
        }

        void AddGroupNodesToCanvas(object[] addVMs)
        {
            foreach (var vm in addVMs)
            {
                var groupNode = new GroupNode(Canvas, Offset, Scale);
                groupNode.DataContext = vm;
                groupNode.Template = GroupNodeTemplate;
                groupNode.ApplyTemplate();

                groupNode.Style = GroupNodeStyle;
                groupNode.Initialize();

                groupNode.MouseDown += GroupNode_MouseDown;
                groupNode.MouseUp += Node_MouseDown;

                Canvas.Children.Add(groupNode);
            }
        }

        T FindConnectorContentInNodes<T>(Node[] nodes, Guid guid) where T : NodeConnectorContent
        {
            foreach (var node in nodes)
            {
                var connectorContent = node.FindNodeConnectorContent(guid);
                if (connectorContent != null)
                {
                    return (T)connectorContent;
                }
            }

            return null;
        }

        Style GetNodeStyle(object dataContext, DependencyObject element)
        {
            if (ItemContainerStyleSelector != null)
            {
                return ItemContainerStyleSelector.SelectStyle(dataContext, element);
            }

            return ItemContainerStyle;
        }

        void PreviewConnect(NodeConnectorContent start, NodeConnectorContent toEnd)
        {
            if (start.CanConnectTo(toEnd) && toEnd.CanConnectTo(start))
            {
                var param = new PreviewConnectCommandParameter(start.Node.Guid, start.Guid, toEnd.Node.Guid, toEnd.Guid);
                PreviewConnectCommand?.Execute(param);
                toEnd.CanConnect = param.CanConnect;
            }
            else
            {
                // connecting to same connect in node connector.
                toEnd.CanConnect = false;
            }

            Cursor = toEnd.CanConnect ? Cursors.Cross : Cursors.No;
        }

        void NodeLink_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var nodeLink = sender as NodeLink;
            _DraggingNodeLinkParam = new DraggingNodeLinkParam(nodeLink, nodeLink.StartConnector);

            _ReconnectGhostNodeLink = nodeLink.CreateGhost();
            _ReconnectGhostNodeLink.Style = NodeLinkStyle ?? NodeLinkAnimationStyle;
            Canvas.Children.Add(_ReconnectGhostNodeLink);

            Cursor = Cursors.Cross;

            nodeLink.ReleaseEndPoint();
        }

        void Node_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _IsNodeSelected = true;

            if (_IsStartDragging || _IsRangeSelecting)
            {
                return;
            }

            UpdateNodeSelection(sender as NodeBase);
        }

        void Node_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;

            if (element != null && element.Tag is NodeConnectorContent connector)
            {
                // clicked on the connector
                var posOnCanvas = connector.GetContentPosition(Canvas);

                var nodeLink = new NodeLink(Canvas, posOnCanvas.X, posOnCanvas.Y, Scale, connector)
                {
                    DataContext = null,
                    Style = NodeLinkStyle ?? NodeLinkAnimationStyle
                };

                _DraggingNodeLinkParam = new DraggingNodeLinkParam(nodeLink, connector);
                Canvas.Children.Add(_DraggingNodeLinkParam.NodeLink);

                Cursor = Cursors.Cross;
            }
            else if (IsOffsetMoveWithMouse(e) == false)
            {
                // clicked on the Node
                StartToMoveDraggingNode(e.GetPosition(Canvas), e.Source as NodeBase);
            }
        }

        void GroupNode_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;

            if (IsOffsetMoveWithMouse(e) == false)
            {
                // clicked on the GroupNode
                StartToMoveDraggingNode(e.GetPosition(Canvas), e.Source as NodeBase);
            }
        }

        void UpdateNodeSelection(NodeBase targetNode)
        {
            BeginUpdateSelectedItems();
            if ((Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) != 0)
            {
                targetNode.IsSelected = !targetNode.IsSelected;
                UpdateSelectedItems(targetNode);
            }
            else
            {
                foreach (var node in Canvas.Children.OfType<Node>())
                {
                    node.IsSelected = node == targetNode;
                    UpdateSelectedItems(node);
                }
            }
            EndUpdateSelectedItems();
        }

        void StartToMoveDraggingNode(Point pos, NodeBase node)
        {
            node.CaptureDragStartPosition();

            _DraggingNodes.Add(node);

            _DragStartPointToMoveNode = pos;

            Cursor = Cursors.SizeAll;
        }

        bool IsOffsetMoveWithMouse(MouseButtonEventArgs e)
        {
            switch (MoveWithMouse)
            {
                case MouseButton.Left:
                    return e.LeftButton == MouseButtonState.Pressed;
                case MouseButton.Middle:
                    return e.MiddleButton == MouseButtonState.Pressed;
                case MouseButton.Right:
                    return e.RightButton == MouseButtonState.Pressed;
                default:
                    throw new InvalidCastException();
            }
        }

        void UpdateSelectedItems(NodeBase node)
        {
            if (SelectedItems.Contains(node.DataContext))
            {
                if (node.IsSelected == false)
                {
                    SelectedItems.Remove(node.DataContext);
                }
            }
            else
            {
                if (node.IsSelected)
                {
                    SelectedItems.Add(node.DataContext);
                }
            }
        }
    }
}
