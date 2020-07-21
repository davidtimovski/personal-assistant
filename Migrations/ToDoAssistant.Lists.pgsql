-- Table: public."ToDoAssistant.Lists"

-- DROP TABLE public."ToDoAssistant.Lists";

CREATE TABLE public."ToDoAssistant.Lists"
(
    "Id" serial NOT NULL,
    "UserId" integer NOT NULL,
    "Name" character varying(50) NOT NULL COLLATE pg_catalog."default",
    "Type" character varying(15) COLLATE pg_catalog."default" NOT NULL DEFAULT 'Regular'::character varying,
    "Order" smallint,
    "NotificationsEnabled" boolean NOT NULL DEFAULT TRUE,
    "IsOneTimeToggleDefault" boolean NOT NULL DEFAULT FALSE,
    "IsArchived" boolean NOT NULL DEFAULT FALSE,
    "CreatedDate" timestamp without time zone NOT NULL,
    "ModifiedDate" timestamp without time zone NOT NULL,
    CONSTRAINT "PK_ToDoAssistant.Lists" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ToDoAssistant.Lists_AspNetUsers_UserId" FOREIGN KEY ("UserId")
    REFERENCES public."AspNetUsers" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."ToDoAssistant.Lists"
    OWNER to personalassistant;

-- Index: IX_ToDoAssistant.Lists_UserId

-- DROP INDEX public."IX_ToDoAssistant.Lists_UserId";

CREATE INDEX "IX_ToDoAssistant.Lists_UserId"
    ON public."ToDoAssistant.Lists" USING btree
    ("UserId")
    TABLESPACE pg_default;