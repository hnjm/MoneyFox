﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GenericServices;
using Microsoft.EntityFrameworkCore;
using MoneyFox.Foundation.Groups;
using MoneyFox.Foundation.Resources;
using MoneyFox.ServiceLayer.Interfaces;
using MoneyFox.ServiceLayer.Parameters;
using MoneyFox.ServiceLayer.QueryObject;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;

namespace MoneyFox.ServiceLayer.ViewModels
{
    public abstract class AbstractCategoryListViewModel : BaseNavigationViewModel
    {
        protected readonly ICrudServicesAsync CrudServices;
        protected readonly IDialogService DialogService;
        
        private ObservableCollection<AlphaGroupListGroup<CategoryViewModel>> source;

        /// <summary>
        ///     Base class for the category list user control
        /// </summary>
        protected AbstractCategoryListViewModel(ICrudServicesAsync crudServices,
                                                IDialogService dialogService,
                                                IMvxLogProvider logProvider,
                                                IMvxNavigationService navigationService) : base(logProvider, navigationService)
        {
            CrudServices = crudServices;
            DialogService = dialogService;
        }

        /// <summary>
        ///     Handle the selection of a CategoryViewModel in the list
        /// </summary>
        protected abstract Task ItemClick(CategoryViewModel category);

        /// <summary>
        ///     Collection with categories alphanumeric grouped by
        /// </summary>
        public ObservableCollection<AlphaGroupListGroup<CategoryViewModel>> CategoryList
        {
            get => source;
            set
            {
                if (source == value) return;
                source = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsCategoriesEmpty));
            }
        }

        public bool IsCategoriesEmpty => !CategoryList?.Any() ?? true;

        /// <summary>
        ///     Deletes the passed CategoryViewModel after show a confirmation dialog.
        /// </summary>
        public MvxAsyncCommand<CategoryViewModel> DeleteCategoryCommand => new MvxAsyncCommand<CategoryViewModel>(DeleteCategory);

        /// <summary>
        ///     Edit the currently selected CategoryViewModel
        /// </summary>
        public MvxAsyncCommand<CategoryViewModel> EditCategoryCommand => new MvxAsyncCommand<CategoryViewModel>(EditCategory);

        /// <summary>
        ///     Selects the clicked CategoryViewModel and sends it to the message hub.
        /// </summary>
        public MvxAsyncCommand<CategoryViewModel> ItemClickCommand  => new MvxAsyncCommand<CategoryViewModel>(ItemClick);

        /// <summary>
        ///     Executes a search for the passed term and updates the displayed list.
        /// </summary>
        public MvxAsyncCommand<string> SearchCommand => new MvxAsyncCommand<string>(Search);

        /// <summary>
        ///     Create and save a new CategoryViewModel group
        /// </summary>
        public MvxAsyncCommand<CategoryViewModel> CreateNewCategoryCommand => new MvxAsyncCommand<CategoryViewModel>(CreateNewCategory);

        /// <inheritdoc />
        public override async void ViewAppearing()
        {
            DialogService.ShowLoadingDialog();
            await Task.Run(async () => await Load());
            DialogService.HideLoadingDialog();
        }

        /// <summary>
        ///     Performs a search with the text in the searchtext property
        /// </summary>
        public async Task Search(string searchText = "")
        {
            List<CategoryViewModel> categories;
            if (!string.IsNullOrEmpty(searchText))
            {
                categories = new List<CategoryViewModel>(
                    await CrudServices
                        .ReadManyNoTracked<CategoryViewModel>()
                        .WhereNameEquals(searchText)
                        .ToListAsync());
            } 
            else
            {
                categories = new List<CategoryViewModel>(await CrudServices
                    .ReadManyNoTracked<CategoryViewModel>()
                    .ToListAsync());
            }
            CategoryList = CreateGroup(categories);
        }

        private async Task Load()
        {
            await Search();
        }

        private async Task EditCategory(CategoryViewModel category)
        {
            await NavigationService.Navigate<ModifyCategoryViewModel, ModifyCategoryParameter>(new ModifyCategoryParameter(category.Id));
        }

        private async Task CreateNewCategory(CategoryViewModel category)
        {
            await NavigationService.Navigate<ModifyCategoryViewModel, ModifyCategoryParameter>(new ModifyCategoryParameter());
        }

        private ObservableCollection<AlphaGroupListGroup<CategoryViewModel>> CreateGroup(List<CategoryViewModel> categories) =>
            new ObservableCollection<AlphaGroupListGroup<CategoryViewModel>>(
                AlphaGroupListGroup<CategoryViewModel>.CreateGroups(categories,
                    CultureInfo.CurrentUICulture,
                    s => string.IsNullOrEmpty(s.Name)
                        ? "-"
                        : s.Name[0].ToString().ToUpper(), itemClickCommand: ItemClickCommand));

        private async Task DeleteCategory(CategoryViewModel categoryToDelete)
        {
            if (await DialogService.ShowConfirmMessage(Strings.DeleteTitle, Strings.DeleteCategoryConfirmationMessage))
            {
                await CrudServices.DeleteAndSaveAsync<CategoryViewModel>(categoryToDelete.Id);
                await Search();
            }
        }
    }
}