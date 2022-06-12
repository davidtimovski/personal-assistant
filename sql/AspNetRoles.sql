CREATE TABLE public."AspNetRoles"
(
    "Id" serial NOT NULL,
    "ConcurrencyStamp" text COLLATE pg_catalog."default",
    "Name" character varying(256) COLLATE pg_catalog."default",
    "NormalizedName" character varying(256) COLLATE pg_catalog."default",
    CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id")
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."AspNetRoles"
    OWNER to personalassistant;

CREATE UNIQUE INDEX "RoleNameIndex"
    ON public."AspNetRoles" USING btree
    ("NormalizedName" COLLATE pg_catalog."default")
    TABLESPACE pg_default;
