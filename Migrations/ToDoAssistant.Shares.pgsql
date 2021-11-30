-- Table: public."ToDoAssistant.Shares"

-- DROP TABLE public."ToDoAssistant.Shares";

CREATE TABLE public."ToDoAssistant.Shares"
(
    "ListId" integer NOT NULL,
    "UserId" integer NOT NULL,
    "IsAdmin" boolean NOT NULL DEFAULT false,
    "IsAccepted" boolean DEFAULT NULL,
    "IsArchived" boolean NOT NULL DEFAULT false,
    "Order" smallint,
    "NotificationsEnabled" boolean NOT NULL DEFAULT TRUE,
    "CreatedDate" timestamp with time zone NOT NULL,
    "ModifiedDate" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_ToDoAssistant.Shares" PRIMARY KEY ("ListId", "UserId"),
    CONSTRAINT "FK_ToDoAssistant.Shares_ToDoAssistant.Lists_ListId" FOREIGN KEY ("ListId")
    REFERENCES public."ToDoAssistant.Lists" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT "FK_ToDoAssistant.Shares_AspNetUsers_UserId" FOREIGN KEY ("UserId")
    REFERENCES public."AspNetUsers" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."ToDoAssistant.Shares"
    OWNER to personalassistant;

-- Index: IX_ToDoAssistant.Shares_ListId

-- DROP INDEX public."IX_ToDoAssistant.Shares_ListId";

CREATE INDEX "IX_ToDoAssistant.Shares_ListId"
    ON public."ToDoAssistant.Shares" USING btree
    ("ListId")
    TABLESPACE pg_default;

-- Index: IX_ToDoAssistant.Shares_UserId

-- DROP INDEX public."IX_ToDoAssistant.Shares_UserId";

CREATE INDEX "IX_ToDoAssistant.Shares_UserId"
    ON public."ToDoAssistant.Shares" USING btree
    ("UserId")
    TABLESPACE pg_default;