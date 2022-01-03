using Domain.Model.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Domain.Model
{
    public class MyModel : BaseModel
    {
        private string pageTitle;
        private Device device;
        private double measurementValue;
        private ObservableCollection<Measurement> measurements;
        private ICommand measureCommand;
        
        public MyModel(string PageTitle)
        {
            this.PageTitle = PageTitle;
            measurements = new ObservableCollection<Measurement>();
        }
        
        public string PageTitle
        {
            get { return pageTitle; }
            set { SetProperty(ref pageTitle, value); }
        }
        
        public ObservableCollection<Measurement> Measurements
        {
            get { return measurements; }
            set { SetProperty(measurements, value, () => measurements = value, "Measurements"); }
        }
        
        public Device Device
        {
            get { return device; }
            set { SetProperty(device, value, () => device = value, "Device"); }
        }
        
        public double MeasurementValue
        {
            get { return measurementValue; }
            set { SetProperty(measurementValue, value, () => measurementValue = value, "MeasurementValue"); }
        }
        
        public ICommand MeasureCommand
        {
            get
            {
                return measureCommand;
            }
            set
            {
                SetProperty(ref measureCommand, value);
            }
        }
    }
}
