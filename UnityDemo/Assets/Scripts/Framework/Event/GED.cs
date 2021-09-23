using Geek.Client;

public enum BaseEventID
{
    ServerListLoaded = -1000,
    NoticeLoaded,
    MainCityDollyCmp,
    LoginHistory,
}

/// <summary>
/// Global Event Dispatcher
/// </summary>
public class GED
{
    public static EventDispatcher NED = new EventDispatcher(); //ÍøÂç
    public static EventDispatcher ED = new EventDispatcher(); //ÓÎÏ·
}