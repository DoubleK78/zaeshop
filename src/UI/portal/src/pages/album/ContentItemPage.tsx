import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { StoreState, useAppDispatch } from "../../store";
import { getContentItemsAsyncThunk } from "../../store/reducers/ContentItemSlice";
import { useSelector } from "react-redux";
import ContentItemBlankUpload from "../../components/content-item/ContentItemBlankUpload";
import ContentItemBulkUpload from "../../components/content-item/ContentItemBulkUpload";
import { useTranslation } from "react-i18next";
import classNames from "classnames";

const CotentItemPage: React.FC = () => {
    const { collectionId } = useParams();

    const dispatch = useAppDispatch();
    const [t] = useTranslation();
    const { contentItems, loading } = useSelector((state: StoreState) => state.contentItem);
    const [isCloudServer, setIsCloudServer] = useState(false);

    useEffect(() => {
        if (collectionId && Number(collectionId)) {
            dispatch(getContentItemsAsyncThunk({ id: Number(collectionId) }));
        }
    }, [dispatch, collectionId]);

    return (
        <>
            <div className="page-wrapper">
                {/* Page Content*/}
                <div className="page-content-tab">
                    <div className="container-fluid">
                        {/* Page-Title */}
                        <div className="row">
                            <div className="col-sm-12">
                                <div className="page-title-box">
                                    <div className="float-end">
                                        <ol className="breadcrumb">
                                            <li className="breadcrumb-item">
                                                <a href="crm-contacts.html#">Dashboard</a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="breadcrumb-item">
                                                <a href="crm-contacts.html#">CMS</a>
                                            </li>
                                            {/*end nav-item*/}
                                            <li className="breadcrumb-item active">Album Detail</li>
                                        </ol>
                                    </div>
                                    <h4 className="page-title">{t("album.title")}</h4>
                                </div>
                                {/*end page-title-box*/}
                            </div>
                            {/*end col*/}
                        </div>
                        {/* end page title end breadcrumb */}
                        <div className="row mb-3">
                            <div className="row">
                                <div className="button-items">
                                    <button
                                        onClick={() => setIsCloudServer(false)}
                                        className={classNames("btn", {
                                            "btn-primary": !isCloudServer,
                                            "btn-outline-primary": isCloudServer
                                        })}><i className="fa-solid fa-house"></i> {t('content_item.upload_local_server')}</button>

                                    <button
                                        onClick={() => setIsCloudServer(true)}
                                        className={classNames("btn", {
                                            "btn-primary": isCloudServer,
                                            "btn-outline-primary": !isCloudServer
                                        })}><i className="fa-solid fa-cloud"></i>{t('content_item.upload_local_server')}</button>
                                </div>
                            </div>
                        </div>
                        {!loading && contentItems?.length === 0 && <ContentItemBlankUpload id={collectionId} isCloudServer={isCloudServer}/>}
                        {!loading && contentItems?.length > 0 && <ContentItemBulkUpload id={collectionId} contentItems={contentItems} isCloudServer={isCloudServer} />}
                    </div>
                </div>
            </div>
        </>
    );
}

export default CotentItemPage;