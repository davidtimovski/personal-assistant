CREATE TABLE public."AspNetUsers"
(
    "Id" serial NOT NULL,
    "AccessFailedCount" integer NOT NULL,
    "ConcurrencyStamp" text COLLATE pg_catalog."default",
    "DateRegistered" timestamp without time zone NOT NULL,
    "Email" character varying(256) COLLATE pg_catalog."default",
    "EmailConfirmed" boolean NOT NULL,
    "Language" character varying(5) COLLATE pg_catalog."default" NOT NULL,
    "ToDoNotificationsEnabled" boolean NOT NULL DEFAULT FALSE,
    "ToDoSoundsEnabled" boolean NOT NULL DEFAULT FALSE,
    "CookingNotificationsEnabled" boolean NOT NULL DEFAULT FALSE,
    "ImperialSystem" boolean NOT NULL DEFAULT FALSE,
    "ImageUri" character varying(255) NOT NULL COLLATE pg_catalog."default",
    "LockoutEnabled" boolean NOT NULL,
    "LockoutEnd" timestamp with time zone,
    "Name" character varying(30) COLLATE pg_catalog."default" NOT NULL,
    "NormalizedEmail" character varying(256) COLLATE pg_catalog."default",
    "NormalizedUserName" character varying(256) COLLATE pg_catalog."default",
    "PasswordHash" text COLLATE pg_catalog."default",
    "PhoneNumber" text COLLATE pg_catalog."default",
    "PhoneNumberConfirmed" boolean NOT NULL,
    "SecurityStamp" text COLLATE pg_catalog."default",
    "TwoFactorEnabled" boolean NOT NULL,
    "UserName" character varying(256) COLLATE pg_catalog."default",
    CONSTRAINT "PK_AspNetUsers" PRIMARY KEY ("Id")
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."AspNetUsers"
    OWNER to personalassistant;

CREATE INDEX "EmailIndex"
    ON public."AspNetUsers" USING btree
    ("NormalizedEmail" COLLATE pg_catalog."default")
    TABLESPACE pg_default;

CREATE UNIQUE INDEX "UserNameIndex"
    ON public."AspNetUsers" USING btree
    ("NormalizedUserName" COLLATE pg_catalog."default")
    TABLESPACE pg_default;
