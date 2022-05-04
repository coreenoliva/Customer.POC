import React from 'react';
import ReactDOM from 'react-dom';
import './Form.css';

class Form extends React.Component{
    constructor(props) {
        super(props);
        this.state = {
            firstName: '',
            lastName:'',
            dateOfBirth:'',
            country:''
        };

        this.handleChange = this.handleChange.bind(this);
        this.handleSubmit = this.handleSubmit.bind(this);
    }

    handleChange(event) {
        const value = event.target.value;
        this.setState({
            [event.target.name]: value
        });
    }

    handleSubmit(event){
        
        
        alert('Form has been submitted! \n\n First Name: ' + this.state.firstName +
            '\n Last Name: ' + this.state.lastName +
            '\n Date Of Birth: ' + this.state.dateOfBirth +
            '\n Country: ' + this.state.country);
        
        
        event.preventDefault();
    }

    render() {
        return (
            <div className="Form">
                Sign Up
                <form onSubmit={this.handleSubmit}>
                    <label>
                        First Name:
                        <input type="text" name="firstName" value={this.state.firstName} onChange={this.handleChange}/>
                    </label>
                    <br/>
                    <label>
                        Last Name:
                        <input type="text" name="lastName" value={this.state.lastName} onChange={this.handleChange}/>
                    </label>
                    <br/>
                    <label>
                        Date Of Birth:
                        <input type="text" name="dateOfBirth" value={this.state.dateOfBirth} onChange={this.handleChange}/>
                    </label>
                    <br/>
                    <label>
                        Country:
                        <input type="text" name="country" value={this.state.country} onChange={this.handleChange}/>
                    </label>
                    <br/>
                    <input type="submit" value="Submit"/>
                </form>
            </div>

        );
    }
}

export default Form;
