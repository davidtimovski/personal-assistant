-- Table: public."Accountant.Accounts"

-- DROP TABLE public."Accountant.Accounts";

CREATE TABLE public."Accountant.Accounts"
(
    "Id" serial NOT NULL,
	"UserId" integer NOT NULL,
    "Name" character varying(30) NOT NULL COLLATE pg_catalog."default",
	"IsMain" boolean NOT NULL DEFAULT FALSE,
    "CreatedDate" timestamp without time zone NOT NULL,
    "ModifiedDate" timestamp without time zone NOT NULL,
    CONSTRAINT "PK_Accountant.Accounts" PRIMARY KEY ("Id"),
	CONSTRAINT "FK_Accountant.Accounts_AspNetUsers_UserId" FOREIGN KEY ("UserId")
    REFERENCES public."AspNetUsers" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."Accountant.Accounts"
    OWNER to personalassistant;

-- Index: IX_Accountant.Accounts_UserId

-- DROP INDEX public."IX_Accountant.Accounts_UserId";

CREATE INDEX "IX_Accountant.Accounts_UserId"
    ON public."Accountant.Accounts" USING btree
    ("UserId")
    TABLESPACE pg_default;
