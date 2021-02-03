var register = new Vue(
    {
        el: '#register',
        data: 
        {
            fields: {
                username: null,
                email: null,
                password: null,
                repeatPassword: null
            },

            fieldNames: null,

            messages: {
                username: null,
                email: null,
                password: null,
                repeatPassword: null
            },

            errors: {
                empty: '[This field is required]',
                username: '[Username already exists]',
                email: '[Please enter a correct email address]',
                password: '[Password must be at least 5 characters long]',
                incorrectRepeat: '[Does not equal password]'
            },

            postFields:{
                Username: null,
                Email: null,
                Password: null
            }
        },

        methods:
        {
            getData: function(event)
            {
                this.fields.username = event.target.elements.username.value;
                this.fields.email = event.target.elements.email.value;
                this.fields.password = event.target.elements.password.value;
                this.fields.repeatPassword = event.target.elements.repeat_password.value;

                if (this.fieldNames == null)
                {
                    this.fieldNames = Object.keys(this.fields);
                }


                for (let i = 0; i < this.fieldNames.length; i++)
                {
                    let fieldName = this.fieldNames[i];

                    if (this.fields[fieldName] == '')
                    {
                        this.messages[fieldName] = this.errors.empty;
                    }
                    else
                    {
                        this.messages[fieldName] = null;
                    }
                }

                //Reset error message for email as it's not required
                this.messages.email = null;

                if (this.fields.password.length < 5)
                {
                    this.messages.password = this.errors.password;
                }

                if (this.fields.password !== this.fields.repeatPassword && this.messages.repeatPassword === null)
                {
                    this.messages.repeatPassword = this.errors.incorrectRepeat;
                }


                let dataReceived = true;

                for (let i = 0; i < this.fieldNames.length; i++)
                {
                    let fieldName = this.fieldNames[i];

                    if (this.messages[fieldName] !== null)
                    {
                        dataReceived = false;
                        break;
                    }
                }

                if (dataReceived)
                {
                    this.postFields.Username = this.fields.username;
                    this.postFields.Email = this.fields.email;
                    this.postFields.Password = this.fields.password;

                    this.postData();
                }
            },

            postData: async function()
            {
                const response = await fetch('http://localhost:5000/api/users/register', {
                    method: 'POST',
                    cache: 'no-cache',
                    credentials: 'omit',
                    redirect: 'follow',
                    headers: {'Content-Type': 'application/json'},
                    body: JSON.stringify(this.postFields)
                  });

                const responseData = await response.json();

                if (response.status === 500)
                {
                    this.messages.username = this.errors.username;
                }
                else
                {
                    localStorage.setItem('currentUser', JSON.stringify(responseData));
                    window.location.href = '../main.html';
                }
            },

            togglePasswordVisibility: function()
            {
                let field1 = document.getElementById("password");
                let field2 = document.getElementById("repeat_password");

                if (field1.type === "password")
                {
                    field1.type = "text";
                } 
                else 
                {
                    field1.type = "password";
                }

                if (field2.type === "password")
                {
                    field2.type = "text";
                } 
                else 
                {
                    field2.type = "password";
                }
            }
        }
    });