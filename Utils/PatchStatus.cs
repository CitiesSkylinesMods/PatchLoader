namespace PatchLoader {
    public class PatchStatus {
        private string _patchName;
        private string _patchDirectory;
        private bool _hasError;
        private string _errorMessage;

        public PatchStatus(string patchName, string patchDirectory) {
            _patchName = patchName;
            _patchDirectory = patchDirectory;
        }

        public PatchStatus(string patchName, string patchDirectory, string errorMessage): this(patchName, patchDirectory) {
            _hasError = true;
            _errorMessage = errorMessage;
        }

        // ReSharper disable once UnusedMember.Global
        public string PatchName {
            get { return _patchName; }
        }

        // ReSharper disable once UnusedMember.Global
        public string PatchDirectory {
            get { return _patchDirectory; }
        }

        // ReSharper disable once UnusedMember.Global
        public string ErrorMessage {
            get { return _errorMessage; }
        }

        // ReSharper disable once UnusedMember.Global
        public bool HasError {
            get { return _hasError; }
            private set { _hasError = value; }
        }

        public void SetError(string message) {
            _hasError = true;
            _errorMessage = message;
        }

        public override string ToString() {
            return $"{_patchName} {_patchDirectory} error? {_hasError} message: {_errorMessage}";
        }
    }
}