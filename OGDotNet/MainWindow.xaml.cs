﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace OGDotNet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        long _cancellationToken = long.MinValue;



        public MainWindow()
        {

            

            InitializeComponent();
            grid.Items.Clear();
        }

        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Update();
        }

        private SearchResults<SecurityDocument> GetThem(string name, string type, int currentPage, long cancellationToken)
        {
            var remoteSecuritySource = new RemoteSecurityMasterResource("http://localhost:8080/jax/");
            var securityMasters = remoteSecuritySource.GetSecurityMasters();
            CancelIfCancelled(cancellationToken);
            foreach (var securityMaster in securityMasters)
            {
                CancelIfCancelled(cancellationToken);
                return securityMaster.Search(name, type, currentPage);
            }
            throw new ArgumentException();
        }

        private void CancelIfCancelled(long cancellationToken)
        {
            if (cancellationToken != Interlocked.Read(ref cancellationToken))
            {
                throw new OperationCanceledException();
            }
        }

        private void Update()
        {
            var value = new object();
            var previous = _cancellationToken+1;
            while (Interlocked.Increment(ref _cancellationToken) != previous)
            {
                previous = _cancellationToken+1;
            }

            string type = typeBox.Text;
            string name = nameBox.Text;
            
            int currentPage = CurrentPage;

            BackgroundWorker worker = new BackgroundWorker();
            ThreadPool.QueueUserWorkItem(delegate
                                             {
                                                 var results = GetThem(name, type, currentPage, previous);
                                                 CancelIfCancelled(previous);
                                                 Dispatcher.BeginInvoke((Action) (() =>
                                                                            {
                                                                                CancelIfCancelled(previous);
                                                                                grid.DataContext = results.Documents.Select(s => s.Security).ToList(); //TODO
                                                                                pageCountLabel.DataContext = results.Paging;
                                                                                currentPageLabel.DataContext = results.Paging;
                                                                            }));
                                             });
            
        }

        private int CurrentPage
        {
            get
            {
                int currentPage;
                if (! int.TryParse(currentPageLabel.Text, out currentPage))
                {
                    currentPage = 1;
                }
                return currentPage;
            }
            set { 
                currentPageLabel.Text = value.ToString();
            }
        }

        private void typeBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (IsLoaded)
                Update();
        }

        

        private void nameBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (IsLoaded)
                Update();
        }

        private void currentPageLabel_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Update();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage++;
            Update();
        }
    }
}
