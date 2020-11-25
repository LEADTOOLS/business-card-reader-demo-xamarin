// *************************************************************
// Copyright (c) 1991-2020 LEAD Technologies, Inc.              
// All Rights Reserved.                                         
// *************************************************************
using BCReaderDemo.Models;
using BCReaderDemo.Utils;
using Leadtools;
using Leadtools.Controls;
using Leadtools.Demos.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;

namespace BCReaderDemo.Extentions
{
   public class ContactDetailsViewModel : BindableObject
   {
      private readonly ContactModel _contact;
      private readonly UpdateBusinessCardPage _page;
      private readonly ImageViewer _imageViewer;
      private readonly BoxView _overlay;
      private readonly int _imageWidth;
      private readonly int _imageHeight;
      private LeadRect _selectedSectionBounds = LeadRect.Empty;

      public static readonly BindableProperty SectionsProperty =
          BindableProperty.CreateAttached("SectionsProperty",
            typeof(IList<Section>),
            typeof(ContactDetailsViewModel),
            null, BindingMode.OneWay, propertyChanged: SectionsChanged);

      public ObservableCollection<Section> Sections
      {
         get;
         private set;
      }

      public ContactDetailsViewModel(UpdateBusinessCardPage detailsPage)
      {
         _contact = detailsPage.ContactItem;
         _page = detailsPage;
         _imageViewer = detailsPage.ImageViewer;

         if (_page.DeskewedImage != null)
         {
            _imageWidth = _page.DeskewedImage.Width;
            _imageHeight = _page.DeskewedImage.Height;
         }
         else
         {
            _imageWidth = _page.ContactItem.ImageWidth;
            _imageHeight = _page.ContactItem.ImageHeight;
         }

         _overlay = _page.FieldOverlay;
         SetContact();

         _page.ImageViewer.TransformChanged += _imageViewer_TransformChanged;
      }

      private static void SectionsChanged(BindableObject source, object oldVal, object newVal)
      {
         var tableView = (TableView)source;
         var newSections = (IList<Section>)newVal;

         tableView.Root.Clear();

         if (newSections == null)
         {
            return;
         }

         foreach (var section in newSections)
         {
            tableView.Root.Add(SectionsFactory.CreateSection(section));
         }
      }

      private void SetContact()
      {
         Section nameSection = new Section(); // Image section
         Section phoneSection = new Section { Header = "Tel" }; // All phone numbers
         Section emailSection = new Section { Header = "Email" }; // Emails
         Section companySection = new Section { Header = "Company" }; // Company/Title/Website
         Section addressSection = new Section { Header = "Address" }; // Address only
         Section otherSection = new Section { Header = "Other" }; // Other fields

         nameSection.Rows.Add(new NameRow()
         {
            Text = _contact.Name.Text,
            ImagePath = _contact.ProfileImage,
            AddImageAction = AddImageAction(),
            OnFocusAction = OnFocusAction(_contact.Name.Bounds),
            OnUnfocusAction = UnfocusAction(),
            RowTappedAction = RowTappedAction("Name", "Name", -1),
            TextChangedAction = TextChangedAction("Name", -1),
         });

         int idx = 0;
         foreach (PhoneField phone in _contact.PhoneNumbers)
         {
            this.AddTextRow(phoneSection, Helpers.PhoneTypeToString(phone.Type), phone.Number, nameof(_contact.PhoneNumbers), idx++, phone.Bounds);
         }

         phoneSection.Rows.Add(new ButtonRow()
         {
            Text = "ADD PHONE",
            RowTappedAction = new Action<EventArgs>(async (e) =>
            {
               List<String> phoneList = new List<string>();
               foreach (string name in Enum.GetNames(typeof(PhoneType)))
               {
                  phoneList.Add(Helpers.PhoneTypeToString((PhoneType)Enum.Parse(typeof(PhoneType), name)));
               }

               string action = await _page.DisplayActionSheet("ADD PHONE", "Cancel", null, phoneList.ToArray());

               if (!String.IsNullOrEmpty(action) && !"Cancel".Equals(action))
               {
                  _contact.PhoneNumbers.Add(new PhoneField(String.Empty, LeadRect.Empty, Helpers.PhoneStringToType(action)));
                  _page.ContactModified = true;
                  SetContact();
               }
            }),
         });

         if (HomePage.CurrentAppData.OptionalFields.Email)
         {
            idx = 0;
            foreach (EmailField email in _contact.Emails)
            {
               this.AddTextRow(emailSection, Helpers.EmailTypeToString(email.Type), email.Email, nameof(_contact.Emails), idx++, email.Bounds);
            }

            emailSection.Rows.Add(new ButtonRow()
            {
               Text = "ADD EMAIL",
               RowTappedAction = new Action<EventArgs>(async (e) =>
               {
                  string[] emailTypes = { "Personal Email", "Work Email" };
                  string action = await _page.DisplayActionSheet("ADD EMAIL", "Cancel", null, emailTypes);
                  if (!String.IsNullOrEmpty(action) && !"Cancel".Equals(action))
                  {
                     _contact.Emails.Add(new EmailField(String.Empty, LeadRect.Empty, Helpers.EmailStringToType(action)));
                     _page.ContactModified = true;
                     SetContact();
                  }
               }),
            });

            if (emailSection.Rows.Count > 0)
               emailSection.Rows.Add(new DividerRow());
         }

         if (HomePage.CurrentAppData.OptionalFields.Company)
         {
            idx = 0;
            foreach (ContactField company in _contact.Companies)
            {
               this.AddTextRow(companySection, "Company", company.Text, nameof(_contact.Companies), idx++, company.Bounds);
            }
         }

         if (HomePage.CurrentAppData.OptionalFields.JobTitle)
         {
            idx = 0;
            foreach (ContactField jobTilte in _contact.JobTitles)
            {
               this.AddTextRow(companySection, "Job Title", jobTilte.Text, nameof(_contact.JobTitles), idx++, jobTilte.Bounds);
            }
         }

         if (HomePage.CurrentAppData.OptionalFields.Website)
         {
            idx = 0;
            foreach (ContactField website in _contact.Websites)
            {
               this.AddTextRow(companySection, "Website", website.Text, nameof(_contact.Websites), idx++, website.Bounds);
            }
         }
         
         companySection.Rows.Add(new ButtonRow()
         {
            Text = "ADD FIELD",
            RowTappedAction = new Action<EventArgs>(async (e) =>
            {
                  //_contact.Websites.Add(new ContactField(String.Empty, LeadRect.Empty));
               string[] fieldTypes = { "Company", "Job Title", "Website" };
               string action = await _page.DisplayActionSheet("ADD FIELD", "Cancel", null, fieldTypes);
               if (!String.IsNullOrEmpty(action) && !"Cancel".Equals(action))
               {
                  if (action == "Company")
                     _contact.Companies.Add(new ContactField(String.Empty, LeadRect.Empty));
                  else if (action == "Job Title")
                     _contact.JobTitles.Add(new ContactField(String.Empty, LeadRect.Empty));
                  else if (action == "Website")
                     _contact.Websites.Add(new ContactField(String.Empty, LeadRect.Empty));
                  _page.ContactModified = true;

                  SetContact();
               }
            }),
         });
         
         if (HomePage.CurrentAppData.OptionalFields.Address)
         {
            idx = 0;
            foreach (ContactField address in _contact.Addresses)
            {
               this.AddTextRow(addressSection, "Address", address.Text, nameof(_contact.Addresses), idx++, address.Bounds);
            }

            if (addressSection.Rows.Count > 0)
               addressSection.Rows.Add(new DividerRow());
         }

         if (HomePage.CurrentAppData.OptionalFields.Group)
         {
            otherSection.Rows.Add(new PickerRow()
            {
               Title = "Group",
               SelectedIndex = HomePage.CurrentAppData.GroupPickerItems.DisplayItems.IndexOf(_contact.Group),
               Items = HomePage.CurrentAppData.GroupPickerItems,
               SelectedIndexChanged = new Action<object, EventArgs>((sender, e) =>
               {
                  Picker picker = (Picker)sender;
                  if (picker.SelectedIndex < picker.Items.Count - 1)
                  {
                     _contact.Group = HomePage.CurrentAppData.GroupPickerItems.DisplayItems[picker.SelectedIndex];
                     _page.ContactModified = true;
                  }
               }),
            });
         }

         if (HomePage.CurrentAppData.OptionalFields.Event)
         {
            otherSection.Rows.Add(new PickerRow()
            {
               Title = "Event",
               SelectedIndex = HomePage.CurrentAppData.EventPickerItems.DisplayItems.IndexOf(_contact.Event),
               Items = HomePage.CurrentAppData.EventPickerItems,
               SelectedIndexChanged = new Action<object, EventArgs>((sender, e) =>
               {
                  Picker picker = (Picker)sender;
                  if (picker.SelectedIndex < picker.Items.Count - 1)
                  {
                     _contact.Event = HomePage.CurrentAppData.EventPickerItems.DisplayItems[picker.SelectedIndex];
                     _page.ContactModified = true;
                  }
               }),
            });
         }

         if (HomePage.CurrentAppData.OptionalFields.Referral)
         {
            otherSection.Rows.Add(new PickerRow()
            {
               Title = "Referral",
               SelectedIndex = HomePage.CurrentAppData.ReferralPickerItems.DisplayItems.IndexOf(_contact.Referral),
               Items = HomePage.CurrentAppData.ReferralPickerItems,
               SelectedIndexChanged = new Action<object, EventArgs>((sender, e) =>
               {
                  Picker picker = (Picker)sender;
                  if (picker.SelectedIndex < picker.Items.Count - 1)
                  {
                     _contact.Referral = HomePage.CurrentAppData.ReferralPickerItems.DisplayItems[picker.SelectedIndex];
                     _page.ContactModified = true;
                  }
               }),
            });
         }

         if (HomePage.CurrentAppData.OptionalFields.Note)
         {
            otherSection.Rows.Add(new NotesRow()
            {
               Title = "Notes",
               Text = _contact.Notes,
               TextChangedAction = new Action<object, EventArgs>((sender, e) =>
               {
                  TextChangedEventArgs args = (TextChangedEventArgs)e;
                  _contact.Notes = args.NewTextValue;
                  _page.ContactModified = true;
               }),

               OnFocusAction = new Action<EventArgs>((e) =>
               {
                  UpdateCarouselViewState(false);
#if __IOS__
                  if (e.GetType() == typeof(FocusEventArgs))
                  {
                     _page.ContentLayoutGrid.RowDefinitions[0].Height = new GridLength(PlatformsConstants.PagesHeaderTitleRowHeight, GridUnitType.Absolute);
                     _page.ContentLayoutGrid.RowDefinitions[1].Height = new GridLength(1.3, GridUnitType.Star);
                     _page.ContentLayoutGrid.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Auto);
                     _page.ContentLayoutGrid.RowDefinitions[3].Height = new GridLength(8.7, GridUnitType.Star);
                  }
#endif // #if __IOS__
               }),

               OnUnfocusAction = new Action<EventArgs>((e) =>
               {
                  UpdateCarouselViewState(true);
#if __IOS__
                  // The Editor field has just lost focus, then set back the main grid rows to their original height
                  _page.ContentLayoutGrid.RowDefinitions[0].Height = new GridLength(PlatformsConstants.PagesHeaderTitleRowHeight, GridUnitType.Absolute);
                  _page.ContentLayoutGrid.RowDefinitions[1].Height = new GridLength(2.5, GridUnitType.Star);
                  _page.ContentLayoutGrid.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Auto);
                  _page.ContentLayoutGrid.RowDefinitions[3].Height = new GridLength(7.5, GridUnitType.Star);
#endif // #if __IOS__
               }),
            });
         }

         if (HomePage.CurrentAppData.OptionalFields.Reminder)
         {
            otherSection.Rows.Add(new NotesRow()
            {
               Title = "Reminder",
               Text = _contact.Reminder,
               TextChangedAction = new Action<object, EventArgs>((sender, e) =>
               {
                  TextChangedEventArgs args = (TextChangedEventArgs)e;
                  _contact.Reminder = args.NewTextValue;
                  _page.ContactModified = true;
               }),

               OnFocusAction = new Action<EventArgs>((e) =>
               {
                  UpdateCarouselViewState(false);
#if __IOS__
                  if (e.GetType() == typeof(FocusEventArgs))
                  {
                     _page.ContentLayoutGrid.RowDefinitions[0].Height = new GridLength(PlatformsConstants.PagesHeaderTitleRowHeight, GridUnitType.Absolute);
                     _page.ContentLayoutGrid.RowDefinitions[1].Height = new GridLength(1.3, GridUnitType.Star);
                     _page.ContentLayoutGrid.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Auto);
                     _page.ContentLayoutGrid.RowDefinitions[3].Height = new GridLength(8.7, GridUnitType.Star);
                  }
#endif // #if __IOS__
               }),

               OnUnfocusAction = new Action<EventArgs>((e) =>
               {
                  UpdateCarouselViewState(true);
#if __IOS__
                  // The Editor field has just lost focus, then set back the main grid rows to their original height
                  _page.ContentLayoutGrid.RowDefinitions[0].Height = new GridLength(PlatformsConstants.PagesHeaderTitleRowHeight, GridUnitType.Absolute);
                  _page.ContentLayoutGrid.RowDefinitions[1].Height = new GridLength(2.5, GridUnitType.Star);
                  _page.ContentLayoutGrid.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Auto);
                  _page.ContentLayoutGrid.RowDefinitions[3].Height = new GridLength(7.5, GridUnitType.Star);
#endif // #if __IOS__
               }),
            });
         }

         if(phoneSection.Rows.Count > 0)
            phoneSection.Rows.Add(new DividerRow());
         if(companySection.Rows.Count > 0)
            companySection.Rows.Add(new DividerRow());

         otherSection.Rows.Add(new DividerRow
         {
            HeightRequest = 80,
            VisibleDivider = false,
         });

         var sections = new List<Section>
         {
             nameSection,
             phoneSection,
             emailSection,
             companySection,
             addressSection,
             otherSection,
         };

         IEnumerable<Section> nonEmptySections = sections.Where(x => x.Rows.Any());
         this.Sections = new ObservableCollection<Section>(nonEmptySections);

         // notify view, that sections have been changed
         this.OnPropertyChanged(nameof(this.Sections));
      }

      private Action<object> AddImageAction()
      {
         return async (obj) =>
         {
            var results = await DependencyService.Get<IPermissions>().VerifyPermissionsAsync(false, PermissionType.Camera);
            if (results == null || results[PermissionType.Camera] != PermissionStatus.Granted)
               return;

            string[] actions = null;
            if (!String.IsNullOrEmpty(_contact.ProfileImage))
            {
               actions = new string[] { "Retake Photo", "Remove Photo" };
               string action = await _page.DisplayActionSheet("Profile Photo", "Cancel", null, actions);

               if (!String.IsNullOrEmpty(action) && !"Cancel".Equals(action))
               {
                  if(action == "Retake Photo")
                  {
                     CameraPage cameraPage = new CameraPage(CameraOperationType.Profile);
                     cameraPage.PictureTaken += _page.CameraPictureTaken;

                     HomePage.Instance.PushCameraPage(cameraPage);
                  }
                  else if(action == "Remove Photo")
                  {
                     if (File.Exists(_contact.ProfileImage))
                     {
                        File.Delete(_contact.ProfileImage);
                     }

                     _contact.ProfileImage = "";
                     SetContact();
                     _page.ContactModified = true;
                     return;
                  }
               }
            }
            else
            {
               CameraPage cameraPage = new CameraPage(CameraOperationType.Profile);
               cameraPage.PictureTaken += _page.CameraPictureTaken;

               HomePage.Instance.PushCameraPage(cameraPage);
            }

         };
      }

      private void _imageViewer_TransformChanged(object sender, EventArgs e)
      {
         if (!_overlay.IsVisible || _selectedSectionBounds.IsEmpty) return;

         LeadRect bounds = _imageViewer.ConvertRect(_imageViewer.ActiveItem, ImageViewerCoordinateType.Image, ImageViewerCoordinateType.Control, _selectedSectionBounds);
         Rectangle overlayBounds = new Rectangle(bounds.Left, bounds.Top, bounds.Width, bounds.Height);
         if (!overlayBounds.Equals(_overlay.Bounds))
            _overlay.Layout(overlayBounds);
      }

      private void AddTextRow(Section section, string title, string text, string propertyName, int index, LeadRect bounds)
      {
         EditorRow row = new EditorRow()
         {
            Title = title,
            Text = text,
         };

         if (title == "Website")
            row.Keyboard = Keyboard.Url;
         else if (section.Header == "Tel")
            row.Keyboard = Keyboard.Telephone;
         else if (section.Header == "Email")
            row.Keyboard = Keyboard.Email;
         else
            row.Keyboard = Keyboard.Default;

         row.OnFocusAction = OnFocusAction(bounds);

         row.OnUnfocusAction = UnfocusAction();

         row.RowTappedAction = RowTappedAction(section.Header, title, index);

         row.TextChangedAction = TextChangedAction(propertyName, index);

         section.Rows.Add(row);
      }

      private Action<EventArgs> UnfocusAction()
      {
         return (e) =>
         {
            UpdateCarouselViewState(true);
#if __IOS__
            // The Entry field has just lost focus, then set back the main grid rows to their original height
            _page.ContentLayoutGrid.RowDefinitions[0].Height = new GridLength(PlatformsConstants.PagesHeaderTitleRowHeight, GridUnitType.Absolute);
            _page.ContentLayoutGrid.RowDefinitions[1].Height = new GridLength(2.5, GridUnitType.Star);
            _page.ContentLayoutGrid.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Auto);
            _page.ContentLayoutGrid.RowDefinitions[3].Height = new GridLength(7.5, GridUnitType.Star);
#endif // #if __IOS__

            if (_contact.FocusDisabled)
               return;

            _overlay.IsVisible = false;

            if(_page.ImageViewer != null)
               _page.ImageViewer.Zoom(ControlSizeMode.FitAlways, 1.0, _imageViewer.DefaultZoomOrigin);
         };
      }

      private Action<object, EventArgs> TextChangedAction(string propertyName, int index)
      {
         return (sender, e) =>
         {
            Entry entry = (Entry)sender;
            PropertyInfo property = _contact.GetType().GetProperty(propertyName);
            if (property != null)
            {
               object propertyValue = property.GetValue(_contact);
               if (propertyValue is ContactField)
               {
                  property.SetValue(_contact, new ContactField(entry.Text, ((ContactField)propertyValue).Bounds));
               }
               else if (propertyName == "Emails")
               {
                  EmailField field = _contact.Emails[index];
                  field.Email = entry.Text;

                  _contact.Emails[index] = field;
               }
               else if (propertyName == "PhoneNumbers")
               {
                  PhoneField field = _contact.PhoneNumbers[index];
                  field.Number = entry.Text;

                  _contact.PhoneNumbers[index] = field;
               }
               else if (propertyName == "Websites")
               {
                  ContactField field = _contact.Websites[index];
                  field.Text = entry.Text;

                  _contact.Websites[index] = field;
               }
               else if (propertyName == "Companies")
               {
                  ContactField field = _contact.Companies[index];
                  field.Text = entry.Text;

                  _contact.Companies[index] = field;
               }
               else if (propertyName == "JobTitles")
               {
                  ContactField field = _contact.JobTitles[index];
                  field.Text = entry.Text;

                  _contact.JobTitles[index] = field;
               }
               else if (propertyName == "Addresses")
               {
                  ContactField field = _contact.Addresses[index];
                  field.Text = entry.Text;

                  _contact.Addresses[index] = field;
               }

               _page.ContactModified = true;
            }
         };
      }

      private Action<EventArgs> RowTappedAction(string header, string title, int index)
      {
         return async (e) =>
         {
            if (header == "Tel")
            {
               List<String> phoneList = new List<string>();
               foreach (string name in Enum.GetNames(typeof(PhoneType)))
               {
                  phoneList.Add(Helpers.PhoneTypeToString((PhoneType)Enum.Parse(typeof(PhoneType), name)));
               }

               string action = await _page.DisplayActionSheet("Change Phone Type", "Cancel", "Delete", phoneList.ToArray());

               if (!String.IsNullOrEmpty(action) && !"Cancel".Equals(action))
               {
                  if (action == "Delete")
                  {
                     _contact.PhoneNumbers.RemoveAt(index);
                  }
                  else
                  {
                     _contact.PhoneNumbers[index] = new PhoneField(_contact.PhoneNumbers[index].Number, _contact.PhoneNumbers[index].Bounds, Helpers.PhoneStringToType(action));
                  }

                  SetContact();
                  _page.ContactModified = true;
               }

               return;
            }
            else if (header == "Email")
            {
               string[] emailTypes = { "Personal Email", "Work Email" };
               string action = await _page.DisplayActionSheet("Change Email Type", "Cancel", "Delete", emailTypes);
               if (!String.IsNullOrEmpty(action) && !"Cancel".Equals(action))
               {
                  if (action == "Delete")
                  {
                     _contact.Emails.RemoveAt(index);
                  }
                  else
                  {
                     _contact.Emails[index] = new EmailField(_contact.Emails[index].Email, _contact.Emails[index].Bounds, Helpers.EmailStringToType(action));
                  }

                  SetContact();
                  _page.ContactModified = true;
               }

               return;
            }
            else if (title == "Website")
            {
               return;
            }

            string[] actions = { "Move", "Swap" };

            string fieldAction = await _page.DisplayActionSheet("Action", "Cancel", title == "Name" ? null : "Delete", actions);

            if (!String.IsNullOrEmpty(fieldAction) && !"Cancel".Equals(fieldAction))
            {

               if (fieldAction == "Delete")
               {
                  if (title == "Company")
                  {
                     _contact.Companies.RemoveAt(index);
                  }
                  else if (title == "Address")
                  {
                     _contact.Addresses.RemoveAt(index);
                  }
                  else if (title == "Job Title")
                  {
                     _contact.JobTitles.RemoveAt(index);
                  }

                  _page.ContactModified = true;
                  SetContact();
                  return;
               }

               List<string> fieldsTypes = new List<string> { "Name", "Company", "Address", "Job Title" };

               if (!HomePage.CurrentAppData.OptionalFields.Address)
                  fieldsTypes.Remove("Address");
               if (!HomePage.CurrentAppData.OptionalFields.Company)
                  fieldsTypes.Remove("Company");
               if (!HomePage.CurrentAppData.OptionalFields.JobTitle)
                  fieldsTypes.Remove("Job Title");

               switch (title)
               {
                  case "Name":
                  case "Company":
                  case "Job Title":
                  case "Address":

                     fieldsTypes.Remove(title);
                     if (fieldsTypes.Count <= 1)
                        return;

                     string actionTitle;
                     if (fieldAction == "Move")
                        actionTitle = "Move To";
                     else
                        actionTitle = "Swap With";

                     string action = await _page.DisplayActionSheet(actionTitle, "Cancel", null, fieldsTypes.ToArray());
                     if (!String.IsNullOrEmpty(action) && !"Cancel".Equals(action))
                     {
                        ContactField fieldToMove = new ContactField();
                        ContactField fieldToSwap = new ContactField();

                        if (title == "Name")
                        {
                           fieldToMove = _contact.Name;
                           _contact.Name = new ContactField();
                        }
                        else if (title == "Company")
                        {
                           fieldToMove = _contact.Companies[index];
                           _contact.Companies.RemoveAt(index);
                        }
                        else if (title == "Job Title")
                        {
                           fieldToMove = _contact.JobTitles[index];
                           _contact.JobTitles.RemoveAt(index);
                        }
                        else if (title == "Address")
                        {
                           fieldToMove = _contact.Addresses[index];
                           _contact.Addresses.RemoveAt(index);
                        }

                        if (fieldAction == "Move")
                        {
                           // Move to
                           if (action == "Name")
                           {
                              _contact.Name = fieldToMove;
                           }
                           else if (action == "Company")
                           {
                              _contact.Companies.Add(fieldToMove);
                           }
                           else if (action == "Job Title")
                           {
                              _contact.JobTitles.Add(fieldToMove);
                           }
                           else if (action == "Address")
                           {
                              _contact.Addresses.Add(fieldToMove);
                           }
                        }
                        else
                        {
                           // Swap with
                           if (action == "Name")
                           {
                              fieldToSwap = _contact.Name;
                              _contact.Name = fieldToMove;
                           }
                           else if (action == "Company")
                           {
                              if (_contact.Companies.Count > 0)
                              {
                                 fieldToSwap = _contact.Companies[_contact.Companies.Count - 1];
                                 _contact.Companies.Remove(fieldToSwap);
                              }

                              _contact.Companies.Add(fieldToMove);
                           }
                           else if (action == "Job Title")
                           {
                              if (_contact.JobTitles.Count > 0)
                              {
                                 fieldToSwap = _contact.JobTitles[_contact.JobTitles.Count - 1];
                                 _contact.JobTitles.Remove(fieldToSwap);
                              }

                              _contact.JobTitles.Add(fieldToMove);
                           }
                           else if (action == "Address")
                           {
                              if (_contact.Addresses.Count > 0)
                              {
                                 fieldToSwap = _contact.Addresses[_contact.Addresses.Count - 1];
                                 _contact.Addresses.Remove(fieldToSwap);
                              }

                              _contact.Addresses.Add(fieldToMove);
                           }


                           if (title == "Name")
                           {
                              _contact.Name = fieldToSwap;
                           }
                           else if (title == "Company")
                           {
                              _contact.Companies.Add(fieldToSwap);
                           }
                           else if (title == "Job Title")
                           {
                              _contact.JobTitles.Add(fieldToSwap);
                           }
                           else if (title == "Address")
                           {
                              _contact.Addresses.Add(fieldToSwap);
                           }
                        }

                        SetContact();
                        _page.ContactModified = true;
                     }
                     break;

                  default:
                     break;
               }
            }
         };
      }

      private Action<EventArgs> OnFocusAction(LeadRect bounds)
      {
         return (e) =>
         {
            if (bounds.IsEmpty)
            {
               _selectedSectionBounds = bounds;
               return;
            }

            UpdateCarouselViewState(false);
#if __IOS__
            if (e.GetType() == typeof(FocusEventArgs))
            {
               _page.ContentLayoutGrid.RowDefinitions[0].Height = new GridLength(PlatformsConstants.PagesHeaderTitleRowHeight, GridUnitType.Absolute);
               _page.ContentLayoutGrid.RowDefinitions[1].Height = new GridLength(1.3, GridUnitType.Star);
               _page.ContentLayoutGrid.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Auto);
               _page.ContentLayoutGrid.RowDefinitions[3].Height = new GridLength(8.7, GridUnitType.Star);
            }
#endif // #if __IOS__

            if (_contact.FocusDisabled)
            {
               return;
            }
            else
            {
               // The Entry field has the focus, then resize the main grid rows to make space for the keyboard
               if (_page.ImageViewer != null)
                  _imageViewer.Zoom(ControlSizeMode.None, 1.0, _imageViewer.DefaultZoomOrigin);
            }

            _overlay.IsVisible = true;

            // Save the last selected BC section/field in order to layout the overlay box inside the TransformChanged event of the ImageViewer control
            _selectedSectionBounds = bounds;

            if (_page.ImageViewer != null)
            {
               LeadMatrix transform = _imageViewer.GetImageTransformWithDpi(true);
               var boundsD = transform.TransformRect(bounds.ToLeadRectD());

               boundsD.Inflate(30, 30);
               _imageViewer.ZoomToRect(boundsD);
            }
         };
      }

      private void UpdateCarouselViewState(bool enable)
      {
         if (_page == null || _page.CarouselView == null)
            return;

         Device.BeginInvokeOnMainThread(() =>
         {
            if (!enable)
               _page.CarouselView.Position = 0;
         });

         _page.CarouselView.ShowIndicators = enable;
         _page.CarouselView.IsSwipeEnabled = enable;
      }
   }
}
