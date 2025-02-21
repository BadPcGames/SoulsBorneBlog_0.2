export async function getUserRole() {
    try {
        const response = await $.ajax({
            url: '/Auth/GetUserRole', 
            type: 'GET'
        });
        return response;
    } catch (error) {
        console.error("Ошибка при получении статуса пользователя:", error);
        return null;
    }
}

export async function getUserId() {
    try {
        const response = await $.ajax({
            url: '/Auth/GetUserId',
            type: 'GET'
        });
        return response;
    } catch (error) {
        console.error("Ошибка при получении статуса пользователя:", error);
        return null;
    }
}

export function checkAccess() {
    return new Promise((resolve, reject) => {
        $.ajax({
            url: '/Auth/getAccess', 
            type: 'GET',
            success: function (response) {
                if (!response) {
                    alert("Ви в бані");
                    resolve(false);
                } else {
                    resolve(true);
                }
            },
            error: function () {
                alert("Ви в бані");
                resolve(false);
            }
        });
    });
}

