namespace CLA.Admin.Beckend
{
    public class ProcessService
    {
        public readonly List<ProcessModel> _list = new List<ProcessModel>();
        public IReadOnlyList<ProcessModel> List => _list.ToList();

        public void Add(ProcessModel model)
        {
            if(_list.Any(x=>x.VersionId == model.VersionId)){
                throw new InvalidOperationException("Already exist record with same version.");
            }
            _list.Add(model);
        }

        public void Update(ProcessModel model)
        {
            if (!_list.Any(x => x.VersionId == model.VersionId))
            {
                throw new InvalidOperationException("Record with given version does not exist.");
            }
            _list.Remove(_list.Single(x => x.VersionId == model.VersionId));
            _list.Add(model);
        }

        public class ProcessModel
        {
            public string VersionId { get; set; }
            public string FileName { get; set; }
            public ProcessStatusType Status { get; set; }
        }

        public enum ProcessStatusType
        {
            Processing,
            Done
        }
    }
}