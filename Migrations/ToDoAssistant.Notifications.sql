-- Table: public."ToDoAssistant.Notifications"

-- DROP TABLE public."ToDoAssistant.Notifications";

CREATE TABLE public."ToDoAssistant.Notifications"
(
    "Id" serial NOT NULL,
    "UserId" integer NOT NULL,
    "ActionUserId" integer NOT NULL,
    "ListId" integer,
    "TaskId" integer,
    "Message" character varying(255) NOT NULL COLLATE pg_catalog."default",
    "IsSeen" boolean NOT NULL DEFAULT false,
    "CreatedDate" timestamp with time zone NOT NULL,
    "ModifiedDate" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_ToDoAssistant.Notifications" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ToDoAssistant.Notifications_AspNetUsers_UserId" FOREIGN KEY ("UserId")
    REFERENCES public."AspNetUsers" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT "FK_ToDoAssistant.Notifications_AspNetUsers_ActionUserId" FOREIGN KEY ("ActionUserId")
    REFERENCES public."AspNetUsers" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT "FK_ToDoAssistant.Notifications_ToDoAssistant.Lists_ListId" FOREIGN KEY ("ListId")
    REFERENCES public."ToDoAssistant.Lists" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE SET NULL,
    CONSTRAINT "FK_ToDoAssistant.Notifications_ToDoAssistant.Tasks_TaskId" FOREIGN KEY ("TaskId")
    REFERENCES public."ToDoAssistant.Tasks" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE SET NULL
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."ToDoAssistant.Notifications"
    OWNER to personalassistant;

-- Index: IX_ToDoAssistant.Notifications_UserId

-- DROP INDEX public."IX_ToDoAssistant.Notifications_UserId";

CREATE INDEX "IX_ToDoAssistant.Notifications_UserId"
    ON public."ToDoAssistant.Notifications" USING btree
    ("UserId")
    TABLESPACE pg_default;
