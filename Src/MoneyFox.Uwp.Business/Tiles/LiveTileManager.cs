﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;
using GenericServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Toolkit.Uwp.Notifications;
using MoneyFox.Foundation.Resources;
using MoneyFox.ServiceLayer.ViewModels;
using MoneyFox.Windows.Business;
using MoneyFox.Windows.Business.Tiles;

namespace MoneyFox.Uwp.Business.Tiles
{
    public class LiveTileManager
    {
        private readonly ICrudServicesAsync crudService;

        private ApplicationDataContainer localsettings = ApplicationData.Current.LocalSettings;
        private const int numberOfPayments = 8;

        public LiveTileManager(ICrudServicesAsync crudService)
        {
            this.crudService = crudService;
        }

        public async Task UpdatePrimaryLiveTile()
        {
            if (await LiveTileHelper.IsPinned())
            {
                object b = localsettings.Values["lastrun"];
                string lastrun = (string)b;
                string headertext = "";
                List<string> displaycontentmedium = new List<string>();
                List<string> displaycontentlarge = new List<string>();

                if (lastrun == "last")
                {
                    localsettings.Values["lastrun"] = "next";
                    headertext = Strings.LiveTileUpcommingPayments;
                    displaycontentmedium = await GetPaymentsAsync(TileSizeOptions.Medium, PaymentInformation.Next);
                    displaycontentlarge = await GetPaymentsAsync(TileSizeOptions.Large, PaymentInformation.Next);
                }
                else
                {
                    localsettings.Values["lastrun"] = "last";
                    headertext = Strings.LiveTilePastPayments;
                    displaycontentmedium = await GetPaymentsAsync(TileSizeOptions.Medium, PaymentInformation.Previous);
                    displaycontentlarge = await GetPaymentsAsync(TileSizeOptions.Large, PaymentInformation.Previous);
                }

                TileContent content = new TileContent()
                {
                    Visual = new TileVisual()
                    {
                        TileMedium = GetTileBinding(headertext, displaycontentmedium),
                        TileWide = GetTileBinding(headertext, displaycontentlarge),
                        TileLarge = GetTileBinding(headertext, displaycontentlarge),
                    }
                };

                TileNotification tn = new TileNotification(content.GetXml());
                TileUpdateManager.CreateTileUpdaterForApplication().Update(tn);
            }
        }

        public async Task UpdateSecondaryLiveTiles()
        {
            var tiles = await SecondaryTile.FindAllForPackageAsync();
            List<string> displaycontent = new List<string>();
            displaycontent = await GetPaymentsAsync(TileSizeOptions.Large, PaymentInformation.Previous);

            if (tiles == null) return;

            foreach (SecondaryTile item in tiles)
            {
                AccountViewModel acct = await crudService.ReadSingleAsync<AccountViewModel>(item.TileId);
                TileContent content = new TileContent()
                {
                    Visual = new TileVisual()
                    {
                        TileSmall = new TileBinding()
                        {
                            Content = new TileBindingContentAdaptive()
                            {
                                Children =
                                {
                            new AdaptiveGroup()
                            {
                                Children =
                                {
                                    new AdaptiveSubgroup()
                                    {
                                        Children =
                                        {
                                            new AdaptiveText()
                                            {
                                                Text = acct.Name,
                                                HintStyle = AdaptiveTextStyle.Caption
                                            },
                                            new AdaptiveText()
                                            {
                                            Text =  LiveTileHelper.TruncateNumber(acct.CurrentBalance),
                                            HintStyle = AdaptiveTextStyle.Caption
                                            }
                                        }
                                    }
                                }
                            }
                            }
                            }
                        },
                        TileMedium = new TileBinding()
                        {
                            Content = new TileBindingContentAdaptive()
                            {
                                Children =
                                {
                            new AdaptiveGroup()
                            {
                                Children =
                                {
                                    new AdaptiveSubgroup()
                                    {
                                        Children =
                                        {
                                    new AdaptiveText()
                                            {
                                                Text = acct.Name,
                                                HintStyle = AdaptiveTextStyle.Caption
                                            },
                                        new AdaptiveText()
                                            {
                                                Text = string.Format(Strings.LiveTileAccountBalance,acct.CurrentBalance.ToString("C2")),
                                                HintStyle = AdaptiveTextStyle.CaptionSubtle
                                            },
                                            new AdaptiveText()
                                            {
                                            Text =  Strings.ExpenseLabel,
                                            HintStyle = AdaptiveTextStyle.Caption
                                            },
                                            new AdaptiveText()
                                            {
                                                Text = string.Format(Strings.LiveTileLastMonthsExpenses, DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(DateTime.Now.AddMonths(-1).Month), LiveTileHelper.TruncateNumber(GetMonthExpenses(DateTime.Now.AddMonths(-1).Month, DateTime.Now.Year,acct))),
                                                HintStyle = AdaptiveTextStyle.CaptionSubtle
                                            },
                                            new AdaptiveText()
                                            {
                                                Text = string.Format(Strings.LiveTileCurrentMonthsExpenses, DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(DateTime.Now.Month),LiveTileHelper.TruncateNumber(GetMonthExpenses(DateTime.Now.Month,DateTime.Now.Year,acct))),
                                                HintStyle = AdaptiveTextStyle.CaptionSubtle
                                            }
                                        }
                                    }
                                }
                            }
                        }
                            }
                        },
                        TileWide = new TileBinding()
                        {
                            Content = new TileBindingContentAdaptive()
                            {
                                Children =
                        {
                            new AdaptiveGroup()
                            {
                                Children =
                                {
                                    new AdaptiveSubgroup()
                                    {
                                        Children =
                                        {
                                            new AdaptiveText()
                                            {
                                                Text = acct.Name,
                                                HintStyle = AdaptiveTextStyle.Caption
                                            },
                                        new AdaptiveText
                                            {
                                                Text = string.Format(Strings.LiveTileAccountBalance,acct.CurrentBalance.ToString("C2")),
                                                HintStyle = AdaptiveTextStyle.CaptionSubtle
                                            },
                                            new AdaptiveText
                                            {
                                            Text =  Strings.ExpenseLabel,
                                            HintStyle = AdaptiveTextStyle.Caption
                                            },
                                            new AdaptiveText
                                            {
                                                Text = string.Format(Strings.LiveTileLastMonthsExpenses, DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(DateTime.Now.AddMonths(-1).Month), GetMonthExpenses(DateTime.Now.AddMonths(-1).Month, DateTime.Now.Year,acct).ToString("C2")),
                                                HintStyle = AdaptiveTextStyle.CaptionSubtle
                                            },
                                            new AdaptiveText
                                            {
                                                Text = string.Format(Strings.LiveTileCurrentMonthsExpenses, DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(DateTime.Now.Month),GetMonthExpenses(DateTime.Now.Month,DateTime.Now.Year,acct).ToString("C2")),
                                                HintStyle = AdaptiveTextStyle.CaptionSubtle
                                            }
                                        }
                                    }
                                }
                            }
                        }
                            }
                        },
                        TileLarge = new TileBinding()
                        {
                            Content = new TileBindingContentAdaptive()
                            {
                                Children =
                        {
                            new AdaptiveGroup()
                            {
                                Children =
                                {
                                    new AdaptiveSubgroup()
                                    {
                                        Children =
                                        {
                                            new AdaptiveText()
                                            {
                                                Text = acct.Name,
                                                HintStyle = AdaptiveTextStyle.Caption
                                            },
                                        new AdaptiveText()
                                            {
                                                Text = string.Format(Strings.LiveTileAccountBalance, acct.CurrentBalance.ToString("C2")),
                                                HintStyle = AdaptiveTextStyle.CaptionSubtle
                                            },
                                            new AdaptiveText()
                                            {
                                            Text =  Strings.ExpenseLabel,
                                            HintStyle = AdaptiveTextStyle.Caption
                                            },
                                            new AdaptiveText()
                                            {
                                                Text = string.Format(Strings.LiveTileLastMonthsExpenses, DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(DateTime.Now.AddMonths(-1).Month),GetMonthExpenses(DateTime.Now.AddMonths(-1).Month, DateTime.Now.Year,acct).ToString("C2")),
                                                HintStyle = AdaptiveTextStyle.CaptionSubtle
                                            },
                                            new AdaptiveText()
                                            {
                                                Text = string.Format(Strings.LiveTileCurrentMonthsExpenses, DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(DateTime.Now.Month),GetMonthExpenses(DateTime.Now.Month,DateTime.Now.Year,acct).ToString("C2")),
                                                HintStyle = AdaptiveTextStyle.CaptionSubtle
                                            },
                                            new AdaptiveText()
                                            {
                                                Text = Strings.LiveTilePastPayments,
                                                HintStyle = AdaptiveTextStyle.Caption
                                            },
                                            new AdaptiveText()
                                            {
                                                Text = displaycontent[0],
                                                HintStyle = AdaptiveTextStyle.CaptionSubtle
                                            },
                                                new AdaptiveText()
                                            {
                                                Text = displaycontent[1],
                                                HintStyle = AdaptiveTextStyle.CaptionSubtle
                                            },
                                            new AdaptiveText()
                                            {
                                                Text = displaycontent[2],
                                                HintStyle = AdaptiveTextStyle.CaptionSubtle
                                            },
                                            new AdaptiveText()
                                            {
                                                Text = displaycontent[3],
                                                HintStyle = AdaptiveTextStyle.CaptionSubtle
                                            }
                                            ,
                                            new AdaptiveText()
                                            {
                                                Text = displaycontent[4],
                                                HintStyle = AdaptiveTextStyle.CaptionSubtle
                                            },
                                            new AdaptiveText()
                                            {
                                                Text = displaycontent[5],
                                                HintStyle = AdaptiveTextStyle.CaptionSubtle
                                            }
                                        }
                                    }
                                }
                            }
                        }
                            }
                        }
                    }
                };

                TileNotification tn = new TileNotification(content.GetXml());
                TileUpdateManager.CreateTileUpdaterForSecondaryTile(item.TileId).Update(tn);
            }
        }

        public double GetMonthExpenses(int month, int year, AccountViewModel account)
        {
            double balance = 0.00;
            List<LiveTilesPaymentInfo> allpayment = new List<LiveTilesPaymentInfo>();
            List<PaymentViewModel> payments = crudService.ReadManyNoTracked<PaymentViewModel>()
                .Where(x => x.ChargedAccountId == account.Id)
                .ToList();

            foreach (PaymentViewModel item in payments)
            {
                if (item.IsRecurring)
                {
                    if (item.Type != Foundation.PaymentType.Income)
                    {
                        allpayment.AddRange(GetReccurance(item));
                    }
                }
                else if (item.Type != Foundation.PaymentType.Income)
                {
                    CreateLiveTileInfos(item, allpayment, item.Date.Date);
                }
            }

            List<LiveTilesPaymentInfo> tiles = allpayment
                .Where(x => x.Date.Date.Month == month && x.Date.Date.Year == year)
                .ToList();

            foreach (LiveTilesPaymentInfo item in tiles)
            {
                balance += item.Amount;
            }
            allpayment.Clear();
            return balance;
        }

        private TileBinding GetTileBinding(string headertext, List<string> displaycontentmedium)
        {
            return new TileBinding
            {
                Content = new TileBindingContentAdaptive
                {
                    Children = {
                        new AdaptiveGroup
                        {
                            Children =
                            {
                                new AdaptiveSubgroup
                                {
                                    Children =
                                    {
                                        new AdaptiveText
                                        {
                                            Text = headertext,
                                            HintStyle = AdaptiveTextStyle.Caption
                                        },
                                        new AdaptiveText
                                        {
                                            Text = displaycontentmedium[0],
                                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                                        },
                                        new AdaptiveText
                                        {
                                            Text = displaycontentmedium[1],
                                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                                        },
                                            new AdaptiveText
                                        {
                                            Text = displaycontentmedium[2],
                                            HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        },
                                        new AdaptiveText
                                        {
                                            Text = displaycontentmedium[3],
                                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                                        },
                                        new AdaptiveText
                                        {
                                            Text = displaycontentmedium[4],
                                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                                        },
                                        new AdaptiveText
                                        {
                                            Text = displaycontentmedium[5],
                                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                                        },
                                        new AdaptiveText
                                        {
                                            Text = displaycontentmedium[6],
                                            HintStyle=AdaptiveTextStyle.CaptionSubtle
                                        },
                                        new AdaptiveText
                                        {
                                            Text = displaycontentmedium[7],
                                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private async Task<List<string>> GetPaymentsAsync(TileSizeOptions tilesize, PaymentInformation paymentInformation)
        {
            List<AccountViewModel> acct = await crudService.ReadManyNoTracked<AccountViewModel>()
                .ToListAsync();
            List<PaymentViewModel> allpayments = new List<PaymentViewModel>();
            List<LiveTilesPaymentInfo> allpayment = new List<LiveTilesPaymentInfo>();

            foreach (AccountViewModel item in acct)
            {
                allpayments.AddRange(crudService.ReadManyNoTracked<PaymentViewModel>()
                    .Where(x => x.ChargedAccountId == item.Id)
                    .ToList());

                allpayments.AddRange(crudService.ReadManyNoTracked<PaymentViewModel>()
                    .Where(x => x.TargetAccountId == item.Id)
                    .ToList());
            }

            foreach (PaymentViewModel item in allpayments)
            {
                if (item.IsRecurring)
                {
                    allpayment.AddRange(GetReccurance(item));
                }
                else
                {
                    var tileinfo = new LiveTilesPaymentInfo();
                    tileinfo.Chargeaccountname = item.ChargedAccount.Name;
                    tileinfo.Amount = item.Amount;
                    tileinfo.Date = item.Date.Date;
                    tileinfo.Type = item.Type;
                    allpayment.Add(tileinfo);
                }
            }

            List<LiveTilesPaymentInfo> payments;

            if (paymentInformation == PaymentInformation.Previous)
            {
                payments = allpayment.OrderByDescending(x => x.Date.Date)
                    .ThenBy(x => x.Date.Date <= DateTime.Today.Date)
                    .Take(numberOfPayments)
                    .ToList();
            }
            else
            {
                payments = allpayment.OrderBy(x => x.Date.Date)
                    .ThenBy(x => x.Date.Date >= DateTime.Today.Date)
                    .Take(numberOfPayments)
                    .ToList();
            }

            List<string> returnlist = payments.Select(x => LiveTileHelper.GetTileText(tilesize, x)).ToList();

            for (int i = returnlist.Count; i < (numberOfPayments - 1); i++)
            {
                returnlist.Add(string.Empty);
            }

            allpayments.Clear();
            return returnlist;
        }

        private List<LiveTilesPaymentInfo> GetReccurance(PaymentViewModel payment)
        {
            List<LiveTilesPaymentInfo> allpayment = new List<LiveTilesPaymentInfo>();

            if (payment.RecurringPayment.IsEndless)
            {
                DateTime startDate = payment.RecurringPayment.StartDate;
                while (DateTime.Compare(DateTime.Now, startDate) <= 0)
                {
                    startDate = CreateLiveTileInfos(payment, allpayment, startDate);
                }
            }
            else
            {
                DateTime startDate = payment.RecurringPayment.StartDate;
                DateTime endDate = payment.RecurringPayment.EndDate.Value;
                while (DateTime.Compare(startDate, endDate) <= 0)
                {
                    startDate = CreateLiveTileInfos(payment, allpayment, startDate);
                }
            }
            return allpayment;
        }

        private DateTime CreateLiveTileInfos(PaymentViewModel payment, List<LiveTilesPaymentInfo> allpayment, DateTime startDate)
        {
            var liveTilesPaymentInfo = new LiveTilesPaymentInfo
            {
                Date = startDate,
                Amount = payment.RecurringPayment?.Amount ?? payment.Amount,
                Chargeaccountname = payment.RecurringPayment == null
                    ? payment.ChargedAccount.Name 
                    : payment.RecurringPayment?.ChargedAccount.Name,
                Type = payment.RecurringPayment?.Type ?? payment.Type
            };
            allpayment.Add(liveTilesPaymentInfo);
            return LiveTileHelper.AddDateByRecurrence(payment, startDate);
        }
    }
}