namespace Geek.Server
{
    public enum EventID
    {
        #region role event
        //玩家事件
        RoleLevelUp = 1001, //玩家等级提升
        RoleVipChange, //玩家vip改变
        OnRoleOnline, //玩家上线
        OnRoleOffline, //玩家下线

        GotNewPet, // 解锁用
        GotNewPets, // 任务用
        DeletePet,

        //任务事件
        PetLevelChange,    // 宠物升级
        ChapterInfoChange, // 主线章节变化
        SenfFirendGift,    // 赠送好友礼物
        ArenaPK,           // 竞技场挑战
        ChapterFastFight,  // 主线快速挑战
        FihgtPowerChange,  // 战力提升
        TrialFight,        // 试炼塔战斗
        Turntable,         // 幸运转盘
        EndlessFight,      // 无尽试炼战斗
        JoinEndless,       // 参与无尽战斗
        JoinVoyage,        // 参加远航
        RefreshVoyage,     // 刷新远航
        FightGuildBoss,    //参加公会boss
        GuildDonate,       //公会捐赠
        FightDailyCopy,    //日常副本
        ComposePet,        //合成宠物
        AddFriend, //添加好友

        TouchGold,          //点金

        IslandChallenge, //公会战挑战

        WarshipsChange, //战船升级
        SeaPatrol_Battle_Victory,//大航海战斗胜利
        SeaPatrolNewLayer,//大航海进入新的一层
        SeaPatrolChoicesPoint,//大航海选择岛屿（悬赏令用（事件ID 1~9））
        WarshipsUnlock, //战船解锁
        ShopRefersh, // 商店刷新
        ShopBuyItem,       //商店购买
        TaskEvent,
        FriendFight,// 好友助战
        UserSpecial, // 用户特权
        CancelUserSpecial, // 取消特权；
        ChongZhiSuccess,
        ChongZhiSuccessAndBeanId,
        RUNE_STAR,//合成
        Lottery, // 抽奖
        LotteryGetHero, // 抽奖英雄
        PetStarUp, // 宠物升星；

        GetGodEquip,// 获得神装
        GetEquip, // 获得装备
        EquipStarUp, // 装备升星
        GetRune, // 得到符文，需要传递符文等级，
        UsePetSkillBook,// 使用 宠物技能书
        ResetPet, // 献祭伙伴
        GetCampMaterial,//获得阵营技能材料

        ChampionGamBle,//冠军赛全部竞猜正确
        ChampionRanking,//冠军赛排名
        ChampionshipSuccessGuess,//冠军赛单场比赛成功竞猜

        TaskAward, //任务完成领奖
        MonthCardActivity, // 月卡激活

        XuanShangExpChange, // 悬赏令经验值变化
        CurXuanShangEnd, // 悬赏令当前赛季结束

        RoleTaskCompelte, // 玩家完成的任务

        RoleActivityChange, // 点金活动变化

        GuildLevelUp, // 公会升级
        GuildBossPlunder, //公会抢夺
        GuildFight, // 公会战
        FuncClose, // 功能关闭
        FuncOpen, //功能开放

        SoloTowerBeatEnemy,//秘境冒险击败怪物
        SoloTowerGoNextStage,//秘境冒险进入下一层

        PassDungeon, // 主线通关
        CampTowerPassCard, // 阵营塔通关

        PetArenaWin, //宠物N挑战竞技场胜利

        ItemAdd,
        IsUnlockSkin,//是否解锁皮肤

        RankMatchSeasonChange,//段位赛赛季变更
        RankMatchStateChange,//段位赛状态变更

        RankMatchDoFight,//段位赛挑战
        PetReset,//宠物重生

        WarshipTeamLevelChange,//战舰系统等级变化
        WarshipPetIn,//宠物进槽
        WarshipPetOut,//宠物出槽

        AwakePet,       //觉醒宠物
        AwakeLevelUp,   //觉醒装备升级
        AwakeStarUp,    //觉醒装备升星
        AwakeSkillUp,   //觉醒技能升级

        TopFinalDispatchedTeamWantedExp,//顶上决战派遣过队伍增加悬赏令经验

        AwakeMove,      //觉醒装备转移 a->b

        RoleGuildChange,//玩家公会变化 <is加入,公会Id>

        StampGroupUp,//邮票组合激活/升级
        StampSkillUp,//邮票技能激活/升级
        StampReset,//邮票重置
        StampGacha,//邮票抽卡
        StampExpUp,//邮票经验

        TopfinalHoldIsland,

        PetQualityUp,
        GetOfflineReard,
        OpenWindow,
        WorldChat,
        CostDiamond,
        SeapatrolFinish,
        PetDressRune,
        EquipRefine,

        FriendApplication,

        MasterLevelChange,
        RoleSignIn,
        ChampionChampion,
        RankChange,
        CampSkillLevelUp,
        SeapatrolHardClearWithoutPotion,
        CrossChampionChampion,

        NewbieTrialAllFinish,
        #endregion

        /// <summary>
        /// 玩家事件分割点
        /// </summary>
        RoleSeparator = 8000,

        #region server event
        //服务器事件
        WorldLevelChange, //世界等级改变
        ReloadBean, //服务器开启时重载配置表

        ActivityDel, // 新活动删除
        ActivityChange, // 新活动更新
        #endregion
    }
}
