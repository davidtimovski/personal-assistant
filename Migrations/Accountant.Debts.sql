-- Table: public."Accountant.Debts"

-- DROP TABLE public."Accountant.Debts";

CREATE TABLE public."Accountant.Debts"
(
    "Id" serial NOT NULL,
	  "UserId" integer NOT NULL,
    "Person" character varying(20) COLLATE pg_catalog."default" NOT NULL,
	  "Amount" decimal(10, 2) NOT NULL,
	  "Currency" character varying(3) COLLATE pg_catalog."default" NOT NULL DEFAULT 'EUR',
    "Description" character varying(255) COLLATE pg_catalog."default",
	  "UserIsDebtor" boolean NOT NULL DEFAULT FALSE,
    "CreatedDate" timestamp with time zone NOT NULL,
    "ModifiedDate" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Accountant.Debts" PRIMARY KEY ("Id"),
	  CONSTRAINT "FK_Accountant.Debts_AspNetUsers_UserId" FOREIGN KEY ("UserId")
    REFERENCES public."AspNetUsers" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."Accountant.Debts"
    OWNER to personalassistant;

-- Index: IX_Accountant.Debts_UserId

-- DROP INDEX public."IX_Accountant.Debts_UserId";

CREATE INDEX "IX_Accountant.Debts_UserId"
    ON public."Accountant.Debts" USING btree
    ("UserId")
    TABLESPACE pg_default;
