using Domain.Business.IServices;
using Domain.Commands;
using Domain.Model;
using Domain.Navigation;
using Domain.ViewModel.Base;
using System;
using System.Threading.Tasks;
 
namespace Domain.ViewModel
{
    public class MyViewModel : BaseMyViewModel
    {
        private readonly IMyService myService;
        private bool StopWorking = false;
        public MyViewModel(INavigationHandler navigationService, SelectedRun selectedRun, IMyService myService) : base(selectedRun)
        {
            this.myService = myService;

            MyModel = new MyModel("My Cool Model")
            {
                MeasureCommand = new DelegateCommand(MeasureClicked),
                Device = myService.GetDevice(),
            };
            
            Start();
        }

        public MyModel MyModel { get; set; }

        public void Start()
        {
            StopWorking = false;
            // thread to update screen with new values
            Task.Run(() =>
            {
                while (!StopWorking)
                {
                    MyModel.MeasurementValue = myService.GetValue();
                }
            });
        }

        public void MeasureClicked(object e)
        {
            StopWorking = true;
            myService.AddMeasurement(MyModel.Measurements);
            Start();
        }
    }
}
