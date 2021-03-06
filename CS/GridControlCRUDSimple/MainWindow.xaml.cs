﻿using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using DevExpress.CRUD.Northwind;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DevExpress.CRUD.Northwind.DataModel;

namespace GridControlCRUDSimple {
    public partial class MainWindow : ThemedWindow {
        public MainWindow() {
            InitializeComponent();
            using(var context = new NorthwindContext()) {
                grid.ItemsSource = context
                    .Products
                    .Select(product => new ProductInfo {
                        Id = product.Id,
                        Name = product.Name,
                        CategoryId = product.CategoryId
                    })
                    .ToList();
                categoriesLookup.ItemsSource = context
                    .Categories
                    .Select(category => new CategoryInfo {
                        Id = category.Id,
                        Name = category.Name,
                    })
                    .ToList();
            }
        }

        void tableView_ValidateRow(object sender, GridRowValidationEventArgs e) {
            var productInfo = (ProductInfo)e.Row;
            using(var context = new NorthwindContext()) {
                Product product;
                if(view.FocusedRowHandle == DataControlBase.NewItemRowHandle) {
                    product = new Product();
                    context.Products.Add(product);
                } else {
                    product = context.Products.SingleOrDefault(p => p.Id == productInfo.Id);
                    if(product == null) {
                        throw new NotImplementedException("The modified row no longer exists in the database. Handle this case according to your requirements.");
                    }
                }
                product.Name = productInfo.Name;
                product.CategoryId = productInfo.CategoryId;
                context.SaveChanges();
                if(view.FocusedRowHandle == DataControlBase.NewItemRowHandle) {
                    productInfo.Id = product.Id;
                }
            }
        }

        void grid_KeyDown(object sender, KeyEventArgs e) {
            if(e.Key == Key.Delete) {
                var productInfo = (ProductInfo)grid.SelectedItem;
                if(productInfo == null)
                    return;
                if(DXMessageBox.Show(this, "Are you sure you want to delete this row?", "Delete Row", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                    return;
                try {
                    using(var context = new NorthwindContext()) {
                        var result = context.Products.Find(productInfo.Id);
                        if(result == null) {
                            throw new NotImplementedException("The modified row no longer exists in the database. Handle this case according to your requirements.");
                        }
                        context.Products.Remove(result);
                        context.SaveChanges();
                        view.Commands.DeleteFocusedRow.Execute(null);
                    }
                } catch(Exception ex) {
                    DXMessageBox.Show(ex.Message);
                }
            }
        }
    }
}
