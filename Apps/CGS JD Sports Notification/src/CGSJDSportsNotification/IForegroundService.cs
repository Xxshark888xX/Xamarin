namespace CGSJDSportsNotification {
    public interface IForegroundService {
        /*void NotificationNewTicket(string title, string message, string link, string icoCountry, string bigTitle = "JD Sports Ticket available!", int id = -1);
        void NotificationWarning(string title, string message, string bigTitle = "Warning", int id = -1);
        void NotificationRemove(int id);*/
        void Start();
        void Stop();
        bool IsRunning();
    }
}