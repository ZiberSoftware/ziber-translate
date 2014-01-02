
    if exists (select 1 from sysobjects where id = OBJECT_ID(N'[FK5C0F21C325A1468B]') AND parent_obj = OBJECT_ID('[TranslateCategory]'))
alter table [TranslateCategory]  drop constraint FK5C0F21C325A1468B


    if exists (select 1 from sysobjects where id = OBJECT_ID(N'[FK5E629AF89BA534E7]') AND parent_obj = OBJECT_ID('[TranslateKey]'))
alter table [TranslateKey]  drop constraint FK5E629AF89BA534E7


    if exists (select 1 from sysobjects where id = OBJECT_ID(N'[FK5E629AF825A1468B]') AND parent_obj = OBJECT_ID('[TranslateKey]'))
alter table [TranslateKey]  drop constraint FK5E629AF825A1468B


    if exists (select 1 from sysobjects where id = OBJECT_ID(N'[FK78B39D95509E6F0B]') AND parent_obj = OBJECT_ID('[Translation]'))
alter table [Translation]  drop constraint FK78B39D95509E6F0B


    if exists (select 1 from sysobjects where id = OBJECT_ID(N'[FK78B39D95FAE9197D]') AND parent_obj = OBJECT_ID('[Translation]'))
alter table [Translation]  drop constraint FK78B39D95FAE9197D


    if exists (select 1 from sysobjects where id = OBJECT_ID(N'[FK78B39D95BA33B491]') AND parent_obj = OBJECT_ID('[Translation]'))
alter table [Translation]  drop constraint FK78B39D95BA33B491


    if exists (select 1 from sysobjects where id = OBJECT_ID(N'[FKDBDA00CFBA33B491]') AND parent_obj = OBJECT_ID('[TranslationVote]'))
alter table [TranslationVote]  drop constraint FKDBDA00CFBA33B491


    if exists (select 1 from sysobjects where id = OBJECT_ID(N'[FKDBDA00CFA6B2BC2A]') AND parent_obj = OBJECT_ID('[TranslationVote]'))
alter table [TranslationVote]  drop constraint FKDBDA00CFA6B2BC2A


    if exists (select * from dbo.sysobjects where id = object_id(N'[Language]') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table [Language]

    if exists (select * from dbo.sysobjects where id = object_id(N'[TranslateCategory]') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table [TranslateCategory]

    if exists (select * from dbo.sysobjects where id = object_id(N'[TranslateKey]') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table [TranslateKey]

    if exists (select * from dbo.sysobjects where id = object_id(N'[TranslateSet]') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table [TranslateSet]

    if exists (select * from dbo.sysobjects where id = object_id(N'[TranslationChangeset]') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table [TranslationChangeset]

    if exists (select * from dbo.sysobjects where id = object_id(N'[Translation]') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table [Translation]

    if exists (select * from dbo.sysobjects where id = object_id(N'[TranslationVote]') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table [TranslationVote]

    if exists (select * from dbo.sysobjects where id = object_id(N'[Translator]') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table [Translator]

    create table [Language] (
        Id INT IDENTITY NOT NULL,
       IsEnabled BIT null,
       IsoCode NVARCHAR(255) null,
       primary key (Id)
    )

    create table [TranslateCategory] (
        Id INT IDENTITY NOT NULL,
       Label NVARCHAR(255) null,
       TranslateSet_id INT null,
       primary key (Id)
    )

    create table [TranslateKey] (
        Id INT IDENTITY NOT NULL,
       Label NVARCHAR(255) null,
       Category_id INT null,
       TranslateSet_id INT null,
       primary key (Id)
    )

    create table [TranslateSet] (
        Id INT IDENTITY NOT NULL,
       Name NVARCHAR(255) null,
       NeedsReviewing INT null,
       NeedsTranslations INT null,
       Reviewed INT null,
       AllTranslations INT null,
       InternalSetName NVARCHAR(255) null,
       primary key (Id)
    )

    create table [TranslationChangeset] (
        Id INT IDENTITY NOT NULL,
       primary key (Id)
    )

    create table [Translation] (
        Id INT IDENTITY NOT NULL,
       Value NTEXT null,
       IsPublished BIT null,
       NeedsAdminReviewing BIT null,
       Language_id INT null,
       Key_id INT null,
       Translator_id INT null,
       primary key (Id)
    )

    create table [TranslationVote] (
        Id INT IDENTITY NOT NULL,
       CreatedAt DATETIME null,
       Rank INT null,
       IsPublished BIT null,
       NeedsReviewing BIT null,
       Translator_id INT null,
       Translation_id INT null,
       primary key (Id)
    )

    create table [Translator] (
        Id INT IDENTITY NOT NULL,
       Name NVARCHAR(255) null,
       EmailAddress NVARCHAR(255) null,
       Salt NVARCHAR(255) null,
       Hash NVARCHAR(255) null,
       Rank INT null,
       IsBlocked BIT null,
       primary key (Id)
    )

    alter table [TranslateCategory] 
        add constraint FK5C0F21C325A1468B 
        foreign key (TranslateSet_id) 
        references [TranslateSet]

    alter table [TranslateKey] 
        add constraint FK5E629AF89BA534E7 
        foreign key (Category_id) 
        references [TranslateCategory]

    alter table [TranslateKey] 
        add constraint FK5E629AF825A1468B 
        foreign key (TranslateSet_id) 
        references [TranslateSet]

    alter table [Translation] 
        add constraint FK78B39D95509E6F0B 
        foreign key (Language_id) 
        references [Language]

    alter table [Translation] 
        add constraint FK78B39D95FAE9197D 
        foreign key (Key_id) 
        references [TranslateKey]

    alter table [Translation] 
        add constraint FK78B39D95BA33B491 
        foreign key (Translator_id) 
        references [Translator]

    alter table [TranslationVote] 
        add constraint FKDBDA00CFBA33B491 
        foreign key (Translator_id) 
        references [Translator]

    alter table [TranslationVote] 
        add constraint FKDBDA00CFA6B2BC2A 
        foreign key (Translation_id) 
        references [Translation]
