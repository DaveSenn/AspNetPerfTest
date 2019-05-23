use super::schema::tasks;

#[derive(Queryable)]
pub struct Task {
    pub id: i32,
    pub text: String,
    pub priority: i32,
}

#[derive(Insertable)]
#[table_name="tasks"]
pub struct NewTask<'a> {
    pub text: &'a String,
    pub priority: &'a i32,
}