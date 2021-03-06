﻿using System.Collections.ObjectModel;
using System.Globalization;
using MoneyFox.Foundation.Groups;
using MoneyFox.Foundation.Resources;
using MoneyFox.ServiceLayer.Utilities;
using MvvmCross.Commands;

namespace MoneyFox.ServiceLayer.ViewModels.DesignTime
{
    public class DesignTimeCategoryListViewModel : ICategoryListViewModel
    {
        public LocalizedResources Resources { get; } = new LocalizedResources(typeof(Strings), CultureInfo.CurrentUICulture);

        public ObservableCollection<AlphaGroupListGroupCollection<CategoryViewModel>> CategoryList =>
            new ObservableCollection<AlphaGroupListGroupCollection<CategoryViewModel>>
            {
                new AlphaGroupListGroupCollection<CategoryViewModel>("A")
                {
                    new CategoryViewModel {Name = "Auto"}
                },
                new AlphaGroupListGroupCollection<CategoryViewModel>("E")
                {
                    new CategoryViewModel {Name = "Einkaufen"}
                }
            };

        public MvxAsyncCommand<CategoryViewModel> ItemClickCommand { get; }
        public MvxAsyncCommand<string> SearchCommand { get; }

        public CategoryViewModel SelectedCategory { get; set; }
        public string SearchText { get; set; }
        public bool IsCategoriesEmpty => false;
    }
}
