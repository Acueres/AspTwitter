var login = new Vue({
    el: '#login',
        data: 
        {
            fields: {
                username: null,
                password: null
            },

            fieldNames: null,

            messages: {
                username: null,
                password: null
            },

            errors: {
                empty: '[This field is required]',
                username: '[Incorrect username]',
                password: '[Incorrect password]'
            },

            postFields:{
                Username: null,
                Password: null
            }
        },

        methods:
        {
            getData: function(event)
            {
                this.fields.username = event.target.elements.username.value;
                this.fields.password = event.target.elements.password.value;

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
                    this.postFields.Password = this.fields.password;

                    this.postData();
                }
            },

            postData: async function()
            {
                const response = await fetch('http://localhost:5000/api/users/login', {
                    method: 'POST',
                    cache: 'no-cache',
                    credentials: 'omit',
                    redirect: 'follow',
                    headers: {'Content-Type': 'application/json'},
                    body: JSON.stringify(this.postFields)
                  });
                
                const responseData = await response.json();
                
                if (response.status === 404)
                {
                    this.messages.username = this.errors.username;
                }
                else if (response.status === 401)
                {
                    this.messages.password = this.errors.password;
                }
                else
                {
                    localStorage.setItem('currentUser', JSON.stringify(responseData));
                    window.location.href = '../main.html';
                }
            },

            togglePasswordVisibility: function()
            {
                let field = document.getElementById("password");

                if (field.type === "password")
                {
                    field.type = "text";
                } 
                else 
                {
                    field.type = "password";
                }
            }
        }
});