namespace Tools.KkzbGrabber
{
    class Provider
    {
        public Provider(string name, Rate rate)
        {
            Name = name;
            Rate = rate;
            IsNew = true;
        }

        public string Name { get; }
        public Rate Rate { get; }
        public bool IsNew { get; set; }
    }
}
