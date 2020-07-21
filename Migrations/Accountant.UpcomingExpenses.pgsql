-- Table: public."Accountant.UpcomingExpenses"

-- DROP TABLE public."Accountant.UpcomingExpenses";

CREATE TABLE public."Accountant.UpcomingExpenses"
(
    "Id" serial NOT NULL,
	"UserId" integer NOT NULL,
    "CategoryId" integer,
	"Amount" decimal(10, 2) NOT NULL,
	"Currency" character varying(3) COLLATE pg_catalog."default" NOT NULL DEFAULT 'EUR',
    "Description" character varying(255) COLLATE pg_catalog."default",
	"Date" date NOT NULL,
	"Generated" boolean NOT NULL DEFAULT FALSE,
    "CreatedDate" timestamp without time zone NOT NULL,
    "ModifiedDate" timestamp without time zone NOT NULL,
    CONSTRAINT "PK_Accountant.UpcomingExpenses" PRIMARY KEY ("Id"),
	CONSTRAINT "FK_Accountant.UpcomingExpenses_AspNetUsers_UserId" FOREIGN KEY ("UserId")
    REFERENCES public."AspNetUsers" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT "FK_Accountant.UpcomingExpenses_Accountant.Accounts_AccountId" FOREIGN KEY ("AccountId")
    REFERENCES public."Accountant.Accounts" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT "FK_Accountant.UpcomingExpenses_Accountant.Categories_CategoryId" FOREIGN KEY ("CategoryId")
    REFERENCES public."Accountant.Categories" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."Accountant.UpcomingExpenses"
    OWNER to personalassistant;

-- Index: IX_Accountant.UpcomingExpenses_UserId

-- DROP INDEX public."IX_Accountant.UpcomingExpenses_UserId";

CREATE INDEX "IX_Accountant.UpcomingExpenses_UserId"
    ON public."Accountant.UpcomingExpenses" USING btree
    ("UserId")
    TABLESPACE pg_default;
	
-- Index: IX_Accountant.UpcomingExpenses_AccountId

-- DROP INDEX public."IX_Accountant.UpcomingExpenses_AccountId";

CREATE INDEX "IX_Accountant.UpcomingExpenses_AccountId"
    ON public."Accountant.UpcomingExpenses" USING btree
    ("AccountId")
    TABLESPACE pg_default;
	
-- Index: IX_Accountant.UpcomingExpenses_CategoryId

-- DROP INDEX public."IX_Accountant.UpcomingExpenses_CategoryId";

CREATE INDEX "IX_Accountant.UpcomingExpenses_CategoryId"
    ON public."Accountant.UpcomingExpenses" USING btree
    ("CategoryId")
    TABLESPACE pg_default;
