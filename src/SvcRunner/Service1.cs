using System.ServiceProcess;

namespace SvcRunner
{
    public partial class Service1 : ServiceBase
    {
        private ProcessRunner _processRunner;

        public Service1()
        {
            _processRunner = new ProcessRunner();
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _processRunner.Start();
        }

        protected override void OnStop()
        {
            _processRunner.Stop();
        }
    }
}
