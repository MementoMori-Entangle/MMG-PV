﻿using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace MMG.ControlParts
{
    /// <summary>
    /// MultiSelectComboBox.xaml の相互作用ロジック
    /// </summary>
    public partial class MultiSelectComboBox : UserControl
    {
        private readonly string SELECT_ALL_STR = Common.Common.SELECT_GUILD_ALL_STR;

        public delegate void OnFruitSelectedEventHandler(OnFruitSelectedEventArgs e);
        public event OnFruitSelectedEventHandler OnFruitSelected;

        private readonly ObservableCollection<Node> _nodeList;
        public MultiSelectComboBox()
        {
            InitializeComponent();
            _nodeList = new ObservableCollection<Node>();
        }

        public class OnFruitSelectedEventArgs : EventArgs
        {
            public string strFruit;
        }

        #region Dependency Properties

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IList), typeof(MultiSelectComboBox),
            new FrameworkPropertyMetadata(null, OnItemsSourceChanged));

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(IList), typeof(MultiSelectComboBox),
            new FrameworkPropertyMetadata(null, OnSelectedItemsChanged));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(MultiSelectComboBox), new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty DefaultTextProperty =
            DependencyProperty.Register("DefaultText", typeof(string), typeof(MultiSelectComboBox), new UIPropertyMetadata(string.Empty));

        public IList ItemsSource
        {
            get { return (IList)GetValue(ItemsSourceProperty); }
            set
            {
                SetValue(ItemsSourceProperty, value);
            }
        }

        public IList SelectedItems
        {
            get { return (IList)GetValue(SelectedItemsProperty); }
            set
            {
                SetValue(SelectedItemsProperty, value);
            }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public string DefaultText
        {
            get { return (string)GetValue(DefaultTextProperty); }
            set { SetValue(DefaultTextProperty, value); }
        }
        #endregion

        #region Events
        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiSelectComboBox control = (MultiSelectComboBox)d;
            control.DisplayInControl();
        }

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MultiSelectComboBox control = (MultiSelectComboBox)d;
            control.SelectNodes();
            control.SetText();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox clickedBox = (CheckBox)sender;

            if (clickedBox.Content != null && clickedBox.Content.ToString() == SELECT_ALL_STR)
            {
                if (clickedBox.IsChecked.HasValue && clickedBox.IsChecked.Value)
                {
                    foreach (var node in _nodeList)
                    {
                        node.IsSelected = true;

                        if (!node.Title.Equals(SELECT_ALL_STR))
                        {
                            var arg = new OnFruitSelectedEventArgs
                            {
                                strFruit = node.Title
                            };
                            OnFruitSelected(arg);
                        }
                    }
                }
                else
                {
                    foreach (var node in _nodeList)
                        node.IsSelected = false;
                }
            }
            else
            {
                var selectedCount = _nodeList.Count(s => s.IsSelected && s.Title != SELECT_ALL_STR);
                var node = _nodeList.FirstOrDefault(i => i.Title == SELECT_ALL_STR);
                if (node != null)
                    node.IsSelected = selectedCount == _nodeList.Count - 1;

                var arg = new OnFruitSelectedEventArgs
                {
                    strFruit = clickedBox.Content.ToString()
                };
                OnFruitSelected(arg);
            }
            SetSelectedItems();
            SetText();
        }
        #endregion

        #region Methods
        private void SelectNodes()
        {
            if (SelectedItems == null)
                return;

            foreach (var item in SelectedItems)
            {
                var node = _nodeList.FirstOrDefault(i => i.Title == item.ToString());
                if (node != null)
                    node.IsSelected = true;
            }
        }
        private void SetSelectedItems()
        {
            SelectedItems.Clear();
            foreach (var node in _nodeList)
            {
                if (!node.IsSelected || node.Title == SELECT_ALL_STR)
                    continue;

                if (ItemsSource.Count <= 0)
                    continue;

                var source = ItemsSource.Cast<object>().ToList();
                SelectedItems.Add(source.FirstOrDefault(i => i.ToString() == node.Title));
            }
        }

        private void DisplayInControl()
        {
            _nodeList.Clear();
            if (ItemsSource.Count > 0)
                _nodeList.Add(new Node(SELECT_ALL_STR));
            foreach (var item in ItemsSource)
            {
                var node = new Node(item.ToString());
                _nodeList.Add(node);
            }
            MultiSelectCombo.ItemsSource = _nodeList;
        }

        private void SetText()
        {
            if (SelectedItems != null)
            {
                var displayText = new StringBuilder();
                foreach (var s in _nodeList)
                {
                    if (s.IsSelected == true && s.Title == SELECT_ALL_STR)
                    {
                        displayText = new StringBuilder();
                        displayText.Append(SELECT_ALL_STR);
                        break;
                    }
                    if (s.IsSelected != true || s.Title == SELECT_ALL_STR)
                        continue;
                    displayText.Append(s.Title);
                    displayText.Append(',');
                }
                Text = displayText.ToString().TrimEnd(new char[] { ',' });
            }
            // set DefaultText if nothing else selected
            if (string.IsNullOrEmpty(Text))
            {
                Text = DefaultText;
            }
        }

        #endregion
    }
    public class Node : INotifyPropertyChanged
    {
        private string _title;
        private bool _isSelected;
        #region ctor
        public Node(string title)
        {
            Title = title;
        }
        #endregion

        #region Properties
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                NotifyPropertyChanged("Title");
            }
        }
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                NotifyPropertyChanged("IsSelected");
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
