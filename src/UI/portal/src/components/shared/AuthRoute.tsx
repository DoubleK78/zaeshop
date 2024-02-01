import { useSelector } from "react-redux";
import { Route, redirect } from "react-router-dom";
import { StoreState } from "../../store";
import { useEffect } from "react";

interface AuthRoleProps {
    path: string;
    Component: React.FC<any>;
    roles: Array<string>;
}

const AuthRole: React.FC<AuthRoleProps> = ({ path, Component, roles }) => {
    const auth = useSelector((state: StoreState) => state.auth);

    useEffect(() => {
        if (!auth.isAuthenticate && !auth.loading && !auth?.roles?.some(r => roles.includes(r))) {
            redirect("/login");
        }
    }, [auth]);

    return (
        <Route path={path} Component={Component} />
    );
}

export default AuthRole;