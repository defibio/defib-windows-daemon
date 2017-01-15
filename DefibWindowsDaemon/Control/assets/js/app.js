var Application = new Vue({
    el: "#vue",
    data: {
        api: "http://localhost:2700/api",
        loading: {
            working: true,
            status: "Fetching previous user details...",
            attemptedLocalStorage: false
        },
        credentials: {
            username: localStorage.getItem("username"),
            password: localStorage.getItem("password"),
            admin: false,
            token: null,
            displayForm: false,
            error: false
        },
        experiment: {
            running: false,
            error: false,
            result: "",
            editor: null
        },
        reset: {
            running: false,
            error: false,
            password: ""
        },
        heartbeats: [],
        info: {
            mode: "view",
            selected: null,
            saving: false
        },
        temporary: {
            name: "",
            key: "",
            interval: 0
        },
        test: {
            running: false,
            error: false,
            result: "",
            editor: null
        },
        users: [],
        createUser: {
            running: false,
            temporary: {
                username: "",
                password: "",
                admin: false
            }
        },
        updateUser: {
            running: false,
            id: null,
            temporary: {
                username: "",
                password: "",
                admin: false
            }
        }
    },
    methods: {
        promptLogin: function () {
            this.loading.working = false;
            this.credentials.displayForm = true;
        },
        fetchPreviousDetails: function () {
            if (localStorage.getItem("username") == null) {
                this.loading.attemptedLocalStorage = true;
                this.loading.status = "Could not login, loading login form...";
                this.promptLogin();
                return;
            }

            this.attemptLoginWithPreviousDetails();
        },
        attemptLoginWithPreviousDetails: function () {
            this.loading.status = "Attempting to login with previous credentials...";
            this.attemptAuthorization(true);

        },
        attemptAuthorization: function (reprompt) {
            var self = this;

            this.$http.get(this.api + "/Authorization/Login?username=" + this.credentials.username + "&password=" + this.credentials.password).then(function (data) {
                if (data.body.type == "InvalidCredentials") {
                    if (reprompt) {
                        self.promptLogin();
                    } else {
                        self.credentials.error = true;
                    }
                } else {
                    if (data.body.type == "ValidatedAdminCredentials") {
                        self.credentials.admin = true;
                    }

                    localStorage.setItem("username", self.credentials.username);
                    localStorage.setItem("password", self.credentials.password);

                    self.loading.working = false;
                    self.credentials.error = false;
                    self.credentials.token = data.body.message;
                    self.credentials.displayForm = false;

                    self.$nextTick(function () {
                        var editor = ace.edit("lua-run-editor");
                        editor.setTheme("ace/theme/twilight");
                        editor.session.setMode("ace/mode/lua");

                        self.experiment.editor = editor;

                        self.loadUsers();
                        self.loadHeartbeats();
                    });
                }
            });
        },
        runExperiment: function () {
            var self = this;

            this.experiment.running = true;
            this.experiment.result = "";

            this.$http.get(this.api + "/Script/Run?token=" + this.credentials.token + "&script=" + self.experiment.editor.getValue()).then(function (data) {
                if (data.body.type == "ScriptError") {
                    self.experiment.error = true;
                }

                self.experiment.result = data.body.message;
                self.experiment.running = false;
            });
        },
        loadHeartbeats: function () {
            var self = this;

            this.$http.get(this.api + "/Heartbeat/List?token=" + this.credentials.token).then(function (data) {
                if (data.body.type == "InvalidToken") {
                    return;
                }

                self.heartbeats = data.body.message;
            });
        },
        createEditor: function () {
            this.$nextTick(function () {
                var editor = ace.edit("lua-create-editor");
                editor.setTheme("ace/theme/twilight");
                editor.session.setMode("ace/mode/lua");

                this.test.editor = editor;
            });
        },
        runTest: function () {
            var self = this;

            this.test.running = true;
            this.test.result = "";

            this.$http.get(this.api + "/Script/Run?token=" + this.credentials.token + "&script=" + self.test.editor.getValue()).then(function (data) {
                if (data.body.type == "ScriptError") {
                    self.test.error = true;
                }

                self.test.result = data.body.message;
                self.test.running = false;
            });
        },
        resetTemporary: function() {
            this.temporary = {
                name: "",
                key: "",
                interval: 0
            };
        },
        submitHeartbeat: function () {
            var self = this;

            if (this.info.mode == 'edit') {
                this.$http.get(this.api + "/Heartbeat/Update?token=" + this.credentials.token + "&id=" + this.temporary.id +
                "&name=" + this.temporary.name + "&key=" + this.temporary.key + "&interval=" + this.temporary.interval +
                "&script=" + this.test.editor.getValue()).then(function (data) {
                    if (data.body.type == "InvalidToken") {
                        return;
                    }

                    self.loadHeartbeats();
                    self.heartbeats.forEach(function (entry) {
                        if (entry.id == parseInt(data.body.message)) {
                            self.info.selected = entry.id;
                            return;
                        }
                    });

                    self.info.mode = 'view';
                    self.temporary = {
                        name: "",
                        key: "",
                        interval: 0
                    };
                });

                return;
            }

            this.$http.get(this.api + "/Heartbeat/Create?token=" + this.credentials.token +
                "&name=" + this.temporary.name + "&key=" + this.temporary.key + "&interval=" + this.temporary.interval +
                "&script=" + this.test.editor.getValue()).then(function (data) {
                if (data.body.type == "InvalidToken") {
                    return;
                }

                self.loadHeartbeats();
                self.heartbeats.forEach(function (entry) {
                    if (entry.id == parseInt(data.body.message)) {
                        self.info.selected = entry;
                        self.info.mode = 'view';
                        self.temporary = {
                            name: "",
                            key: "",
                            interval: 0
                        };

                        return;
                    }
                });
            });
        },
        selectHeartbeatForInfo: function (id) {
            var self = this;

            this.heartbeats.forEach(function (entry) {
                if (entry.id == id) {
                    self.info.selected = entry;
                    return;
                }
            });

            this.info.view = 'view';
        },
        selectHeartbeatForEdit: function (id) {
            var self = this;

            this.heartbeats.forEach(function (entry) {
                if (entry.id == id) {
                    self.temporary = entry;
                    return;
                }
            });

            this.info.view = 'edit';
        },
        selectHeartbeatForDelete: function (id) {
            if (confirm("Are you sure that you want remove this Heartbeat? This can not be reversed.")) {
                this.$http.get(this.api + "/Heartbeat/Delete?token=" + this.credentials.token + "&id=" + id).then(function (data) {
                    if (data.body.type == "InvalidToken") {
                        return;
                    }
                    alert("The heartbeat was successfuly removed.");
                });
            }

            this.loadHeartbeats();
        },
        changePassword: function () {
            if (!this.reset.password.length) {
                return;
            }

            this.$http.get(this.api + "/User/Password?token=" + this.credentials.token + "&password=" + this.reset.password).then(function (data) {
                if (data.body.type == "InvalidToken") {
                    return;
                }

                alert("Your password has successfully been changed.");
                $(".modal-password").modal('hide');
            });
        },
        loadUsers: function () {
            if (this.credentials.admin) {
                var self = this;

                this.$http.get(this.api + "/User/List?token=" + this.credentials.token).then(function (data) {
                    if (data.body.type == "InvalidToken") {
                        return;
                    }

                    self.users = data.body.message;
                });
            }
        },
        updateUser: function () {

        },
        createUser: function () {

        },
        selectUserForEdit: function (id) {
            var self = this;

            this.users.forEach(function (entry) {
                if (entry.id == id) {
                    self.updateUser.temporary = entry;
                    self.updateUser.temporary.password = "";
                    return;
                }
            });

            $('.modal-users').modal('hide');
            $('.modal-user-update').modal('show');
        },
    }
});

$(function () {
    Application.attemptLoginWithPreviousDetails();
});