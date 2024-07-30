import { useTranslation } from "react-i18next";
import UserRoleSubscriptionHistoryAction from "../../models/user/UserRoleSubscriptionHistoryAction";
import UserRoleSubscriptionResponse from "../../models/user/UserRoleSubscriptionResponse";
import dayjsCustom from "../../utils/dayjs/dayjs-custom";
import classNames from "classnames";

type Props = {
    closeModal: (isReload?: boolean) => void;
    onSubmit: () => Promise<void>;
    roleSubscription: UserRoleSubscriptionResponse | null;
    activyHistory: UserRoleSubscriptionHistoryAction
}

const RoleSubscriptionPopup: React.FC<Props> = ({ closeModal, onSubmit, roleSubscription, activyHistory }) => {
    const [t] = useTranslation();

    const EstimateDayChanges = () => {
        const experiedDateDisplay: string = roleSubscription?.role === 'User' ? t('role_subscription.none') : t('role_subscription.forever');
        let neưExperiedDateDisplay: string = '';

        // Case 1: New role subscription from user -> DateTime.UtcNow + days
        // Case 2: Same role subscription -> Increase expries date
        // Case 3: New role subscription from user premium to user super premium
        // Calculate and convert expries date of current role user premium = 10 % remaining days and add days 
        if (roleSubscription?.role === 'User' && activyHistory.role !== 'User') {
            neưExperiedDateDisplay = dayjsCustom.utc().add(activyHistory?.day ?? 0, 'day').local().format('DD-MM-YYYY HH:mm')
        } else if (roleSubscription?.role === activyHistory.role) {
            neưExperiedDateDisplay = dayjsCustom.utc(roleSubscription?.expriedRoleDate ?? new Date()).add(activyHistory?.day ?? 0, 'day').local().format('DD-MM-YYYY HH:mm')
        } else if (roleSubscription?.role === 'User Premium' && activyHistory.role === 'User Super Premium') {
            const remainDays = Math.abs(dayjsCustom.utc().diff(dayjsCustom.utc(roleSubscription?.expriedRoleDate ?? new Date()), 'day'));
            neưExperiedDateDisplay = dayjsCustom.utc().add(remainDays * 0.1 + (activyHistory?.day ?? 0), 'day').local().format('DD-MM-YYYY HH:mm')
        }

        return (
            <p className="met-user-name-post">
                {t('role_subscription.day')}: {' '}
                {!roleSubscription?.expriedRoleDate ? experiedDateDisplay + ' -> ' : (
                    <span className="text-success">
                        {dayjsCustom.utc(roleSubscription?.expriedRoleDate).local().format('DD-MM-YYYY HH:mm')}{' -> '}
                    </span>
                )}
                {!activyHistory?.day ? experiedDateDisplay : (
                    <span className="text-success">
                        {neưExperiedDateDisplay}
                        {' '}<span>(+{activyHistory?.day}) </span>
                    </span>
                )}
            </p>
        )
    }

    return (
        <>
            <div
                className="modal fade show d-block"
                id="exampleModalLogin"
                tabIndex={-1}
                role="dialog"
                aria-labelledby="exampleModalDefaultLogin"
                aria-hidden="true"
            >
                <div className="modal-dialog" role="document">
                    <div className="modal-content">
                        <div className="modal-header">
                            <h6 className="modal-title m-0" id="exampleModalDefaultLogin">
                                {t('role_subscription.update_subscription')}
                            </h6>
                            <button
                                type="button"
                                className="btn-close"
                                data-bs-dismiss="modal"
                                aria-label="Close"
                                onClick={() => closeModal()}
                            />
                        </div>
                        {/*end modal-header*/}
                        <div className="modal-body">
                            <div className="card-body">
                                <div className="row">
                                    <h5>{t('user.modal.are_you_sure')}</h5>
                                    <small className="badge bg-soft-secondary">{t('role_subscription.update_subscription')}</small>
                                    <small className="text-muted ml-2">{dayjsCustom.utc().local().format('DD/MM/YYYY')}</small>

                                    <p className="mb-0 mt-2 met-user-name-post">
                                        {t('role_subscription.role')}: {roleSubscription?.role}{' -> '}
                                        {roleSubscription?.role !== activyHistory.role && activyHistory.role === 'User Premium'}
                                        <span className={classNames({
                                            'text-success': roleSubscription?.role !== activyHistory.role && activyHistory.role === 'User Super Premium'
                                        })}>{activyHistory.role}</span>
                                    </p>
                                    {EstimateDayChanges()}
                                </div>
                                {/*end row*/}
                            </div>
                            {/*end card-body*/}
                        </div>
                        {/*end modal-body*/}
                        <div className="modal-footer">
                            <button
                                type="button"
                                className="btn btn-de-secondary btn-sm"
                                data-bs-dismiss="modal"
                                onClick={() => closeModal()}
                            >
                                {t('user.modal.close')}
                            </button>
                            <button type="submit" className="btn btn-primary btn-sm"
                                onClick={onSubmit}>
                                {t('user.modal.save_changes')}
                            </button>
                        </div>
                        {/*end modal-footer*/}
                    </div>
                    {/*end modal-content*/}
                </div>
                {/*end modal-dialog*/}
            </div>
            {/*end modal*/}
        </>
    );
};

export default RoleSubscriptionPopup;