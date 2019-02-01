using TihiroRobotBeer.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(Config))]
namespace TihiroRobotBeer.Droid {
    public class Config : IConfig {
        private string _diretorioDB { get; set; }
        public string DiretorioDB {
            get {
                if (string.IsNullOrEmpty(_diretorioDB)) {
                    _diretorioDB = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                }
                return _diretorioDB;
            }
        }
    }
}