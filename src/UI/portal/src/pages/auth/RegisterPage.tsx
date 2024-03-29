import React from "react";
import { Link } from "react-router-dom";

const RegisterPage: React.FC = () => {
    return (
        <div className="">
            {/* Section: Design Block */}
            <section className="text-center">
                {/* Background image */}
                <div
                    className="p-5 bg-image"
                    style={{
                        backgroundImage:
                            'url("https://mdbootstrap.com/img/new/textures/full/171.jpg")',
                        height: 250
                    }}
                />
                {/* Background image */}
                <div
                    className="card mx-4 mx-md-5 shadow-5-strong"
                    style={{
                        marginTop: "-100px",
                        background: "hsla(0, 0%, 100%, 0.8)",
                        backdropFilter: "blur(30px)"
                    }}
                >
                    <div className="card-body py-5 px-md-5">
                        {/* Pills navs */}
                        <ul className="nav nav-pills nav-justified mb-3" id="ex1" role="tablist">
                            <li className="nav-item" role="presentation">
                                <Link className="nav-link" to={"/login"}>
                                    Login
                                </Link>
                            </li>
                            <li className="nav-item" role="presentation">
                                <Link className="nav-link active" to={"/register"}>
                                    Register
                                </Link>
                            </li>
                        </ul>
                        {/* Pills navs */}
                        {/* Pills content */}
                        <div className="tab-content">
                            <div className="tab-pane fade show active">
                                <form>
                                    {/* Name input */}
                                    <div className="form-outline mb-4">
                                        <input type="text" id="registerName" className="form-control" />
                                        <label className="form-label" htmlFor="registerName">
                                            Name
                                        </label>
                                    </div>
                                    {/* Username input */}
                                    <div className="form-outline mb-4">
                                        <input type="text" id="registerUsername" className="form-control" />
                                        <label className="form-label" htmlFor="registerUsername">
                                            Username
                                        </label>
                                    </div>
                                    {/* Email input */}
                                    <div className="form-outline mb-4">
                                        <input type="email" id="registerEmail" className="form-control" />
                                        <label className="form-label" htmlFor="registerEmail">
                                            Email
                                        </label>
                                    </div>
                                    {/* Password input */}
                                    <div className="form-outline mb-4">
                                        <input
                                            type="password"
                                            id="registerPassword"
                                            className="form-control"
                                        />
                                        <label className="form-label" htmlFor="registerPassword">
                                            Password
                                        </label>
                                    </div>
                                    {/* Repeat Password input */}
                                    <div className="form-outline mb-4">
                                        <input
                                            type="password"
                                            id="registerRepeatPassword"
                                            className="form-control"
                                        />
                                        <label className="form-label" htmlFor="registerRepeatPassword">
                                            Repeat password
                                        </label>
                                    </div>
                                    {/* Checkbox */}
                                    <div className="form-check d-flex justify-content-center mb-4">
                                        <input
                                            className="form-check-input me-2"
                                            type="checkbox"
                                            defaultValue=""
                                            id="registerCheck"
                                            aria-describedby="registerCheckHelpText"
                                        />
                                        <label className="form-check-label" htmlFor="registerCheck">
                                            I have read and agree to the terms
                                        </label>
                                    </div>
                                    {/* Submit button */}
                                    <button type="submit" className="btn btn-primary btn-block mb-3">
                                        Sign in
                                    </button>
                                </form>
                            </div>
                        </div>
                        {/* Pills content */}
                    </div>
                </div>
            </section>
        </div>
    );
};

export default RegisterPage;