namespace SecretAdmin.Features.Program
{
    public static class ArgumentsManager
    {
        // TODO: this
        /*
         * Arguments:
         * --reconfigure -r
         * --config <filename> -c
         * --no-logs -nl
         */

        public static Args GetArgs(string[] args)
        {
            var ret = new Args();
            
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--reconfigure" or "-r":
                        ret.Reconfigure = true;
                        break;
                    case "--config" or "-c" when args.Length > i + 1:
                        ret.Config = args[i + 1];
                        break;
                    case "--no-logs" or "-nl":
                        ret.Logs = false;
                        break;
                }
            }

            return ret;
        }

        public class Args
        {
            public bool Reconfigure = false;
            public string Config = "default.yml";
            public bool Logs = true;
        }
    }
}